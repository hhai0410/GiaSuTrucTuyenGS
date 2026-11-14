"use strict";
// FullCalendar displays Vietnam wall time; API and database always use real UTC.
const calendarElement = document.querySelector("#availability-calendar");
if (calendarElement && window.FullCalendar) {
    const offset = 7 * 3600000;
    const calendar = new FullCalendar.Calendar(calendarElement, {
        initialView: window.matchMedia("(max-width: 700px)").matches ? "listWeek" : "timeGridWeek",
        initialDate: calendarElement.dataset.week,
        timeZone: "UTC", firstDay: 1, allDaySlot: false, nowIndicator: true,
        now: () => new Date(Date.now() + offset).toISOString(),
        height: 650, slotMinTime: "00:00:00", slotMaxTime: "24:00:00", scrollTime: "08:00:00",
        headerToolbar: { left: "prev,next today", center: "title", right: "timeGridWeek,listWeek,dayGridMonth" },
        buttonText: { today: "Hôm nay", week: "Tuần", month: "Tháng", list: "Danh sách" },
        locale: "vi",
        slotLabelFormat: { hour: "2-digit", minute: "2-digit", hour12: false },
        eventTimeFormat: { hour: "2-digit", minute: "2-digit", hour12: false },
        events: async (info, success, failure) => {
            try {
                const url = new URL(calendarElement.dataset.events, location.origin);
                url.searchParams.set("start", new Date(info.start.getTime() - offset).toISOString());
                url.searchParams.set("end", new Date(info.end.getTime() - offset).toISOString());
                const response = await fetch(url);
                if (!response.ok) throw new Error("calendar");
                const events = await response.json();
                success(events.map(event => {
                    // Omit a missing URL: FullCalendar would otherwise stringify null to /null.
                    const { url: editUrl, ...data } = event;
                    return { ...data, ...(editUrl ? { url: editUrl } : {}),
                        start: new Date(Date.parse(event.start) + offset).toISOString(),
                        end: new Date(Date.parse(event.end) + offset).toISOString() };
                }));
                document.querySelector("#calendar-error").hidden = true;
            } catch (error) {
                document.querySelector("#calendar-error").hidden = false;
                failure(error);
            }
        }
    });
    calendar.render();
}
