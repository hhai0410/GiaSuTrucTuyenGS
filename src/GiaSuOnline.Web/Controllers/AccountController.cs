using System.Text;
using GiaSuOnline.Web.Models;
using GiaSuOnline.Web.ViewModels;
using GiaSuOnline.Web.Integrations.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
namespace GiaSuOnline.Web.Controllers;
public class AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn,
    IEmailSender email, IConfiguration config, ILogger<AccountController> logger) : Controller
{
    [HttpGet, AllowAnonymous] public IActionResult Register() => View(new RegisterForm());
    [HttpPost, AllowAnonymous] public async Task<IActionResult> Register(RegisterForm form)
    {
        if (!ModelState.IsValid) return View(form);
        if (await users.FindByEmailAsync(form.Email.Trim()) != null)
        { ModelState.AddModelError("Email", "Email đã được sử dụng"); return View(form); }
        var user = new ApplicationUser { Email = form.Email.Trim(), UserName = form.Email.Trim(), FullName = form.FullName.Trim() };
        var result = await users.CreateAsync(user, form.Password);
        if (!result.Succeeded) { Errors(result); return View(form); }
        result = await users.AddToRoleAsync(user, form.Role);
        if (!result.Succeeded) { await users.DeleteAsync(user); Errors(result); return View(form); }
        await SendConfirmation(user);
        return RedirectToAction(nameof(Message), new { text = "Đã tạo tài khoản. Hãy mở email xác nhận trước khi đăng nhập. Nếu chưa nhận được, dùng mục Gửi lại email xác nhận." });
    }
    private string Link(string action, ApplicationUser user, string token) =>
        config["PublicOrigin"]!.TrimEnd('/') + Url.Action(action, "Account", new { email = user.Email, token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token)) });
    private async Task SendConfirmation(ApplicationUser user)
    {
        await TrySend(user.Email!, "GS – Xác nhận tài khoản", "Mở liên kết để xác nhận email (hiệu lực 30 phút):\n" + Link(nameof(ConfirmEmail), user, await users.GenerateEmailConfirmationTokenAsync(user)));
    }
    private async Task TrySend(string to, string subject, string body)
    {
        try { await email.SendAsync(to, subject, body); }
        catch (Exception ex) { logger.LogError("Gửi email thất bại ({Type}). Kiểm tra cấu hình SMTP.", ex.GetType().Name); TempData["Warning"] = "Chưa gửi được email. Kiểm tra SMTP rồi gửi lại email xác nhận hoặc yêu cầu đặt lại mật khẩu."; }
    }
    [HttpGet, AllowAnonymous] public async Task<IActionResult> ConfirmEmail(string email, string token)
    {
        var user = await users.FindByEmailAsync(email);
        if (user == null) return BadRequest("Liên kết không hợp lệ.");
        try
        {
            var r = await users.ConfirmEmailAsync(user, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)));
            if (!r.Succeeded) return BadRequest("Liên kết không hợp lệ hoặc đã hết hạn.");
        }
        catch (FormatException) { return BadRequest("Liên kết không hợp lệ."); }
        return RedirectToAction(nameof(Message), new { text = "Email đã xác nhận. Bạn có thể đăng nhập." });
    }
    [HttpGet, AllowAnonymous] public IActionResult ResendConfirmation() => View("EmailForm", new ForgotForm());
    [HttpPost, AllowAnonymous] public async Task<IActionResult> ResendConfirmation(ForgotForm form)
    {
        if (!ModelState.IsValid) return View("EmailForm", form);
        var user = await users.FindByEmailAsync(form.Email);
        if (user != null && !user.EmailConfirmed) await SendConfirmation(user);
        return RedirectToAction(nameof(Message), new { text = "Nếu tài khoản cần xác nhận, email sẽ được gửi." });
    }
    [HttpGet, AllowAnonymous] public IActionResult Login(string? returnUrl) => View(new LoginForm { ReturnUrl = returnUrl });
    [HttpPost, AllowAnonymous] public async Task<IActionResult> Login(LoginForm form)
    {
        if (!ModelState.IsValid) return View(form);
        var user = await users.FindByEmailAsync(form.Email);
        var result = user == null ? Microsoft.AspNetCore.Identity.SignInResult.Failed :
            await signIn.PasswordSignInAsync(user, form.Password, false, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.IsLockedOut ? "Tài khoản bị khóa 15 phút do nhập sai 5 lần." : "Email hoặc mật khẩu chưa đúng, hoặc email chưa được xác nhận.");
            return View(form);
        }
        if (Url.IsLocalUrl(form.ReturnUrl)) return LocalRedirect(form.ReturnUrl!);
        if (await users.IsInRoleAsync(user!, "Admin")) return RedirectToAction("Index", "Profiles", new { area = "Admin" });
        if (await users.IsInRoleAsync(user!, "Tutor")) return RedirectToAction("Mine", "TutorProfiles");
        return RedirectToAction("Index", "Home");
    }
    [HttpPost, Authorize] public async Task<IActionResult> Logout()
    {
        var user = await users.GetUserAsync(User);
        if (user != null) await users.UpdateSecurityStampAsync(user);
        await signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
    [HttpGet, AllowAnonymous] public IActionResult ForgotPassword() => View("EmailForm", new ForgotForm());
    [HttpPost, AllowAnonymous] public async Task<IActionResult> ForgotPassword(ForgotForm form)
    {
        if (!ModelState.IsValid) return View("EmailForm", form);
        var user = await users.FindByEmailAsync(form.Email);
        if (user != null && user.EmailConfirmed)
            await TrySend(user.Email!, "GS – Đặt lại mật khẩu", "Liên kết có hiệu lực 30 phút và chỉ dùng một lần:\n" + Link(nameof(ResetPassword), user, await users.GeneratePasswordResetTokenAsync(user)));
        return RedirectToAction(nameof(Message), new { text = "Nếu email hợp lệ và đã xác nhận, hướng dẫn đặt lại mật khẩu sẽ được gửi." });
    }
    [HttpGet, AllowAnonymous] public IActionResult ResetPassword(string email, string token) => View(new ResetForm { Email = email, Token = token });
    [HttpPost, AllowAnonymous] public async Task<IActionResult> ResetPassword(ResetForm form)
    {
        if (!ModelState.IsValid) return View(form);
        var user = await users.FindByEmailAsync(form.Email);
        if (user == null) { ModelState.AddModelError("", "Liên kết không hợp lệ."); return View(form); }
        try
        {
            var result = await users.ResetPasswordAsync(user, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(form.Token)), form.Password);
            if (!result.Succeeded) { ModelState.AddModelError("", "Liên kết hết hạn/đã dùng hoặc mật khẩu không hợp lệ."); return View(form); }
        }
        catch (FormatException) { ModelState.AddModelError("", "Liên kết không hợp lệ."); return View(form); }
        return RedirectToAction(nameof(Message), new { text = "Đã đặt lại mật khẩu. Hãy đăng nhập lại." });
    }
    [HttpGet, AllowAnonymous] public IActionResult Message(string text) => View(model: text);
    private void Errors(IdentityResult result) { foreach (var e in result.Errors) ModelState.AddModelError("", e.Description); }
}
