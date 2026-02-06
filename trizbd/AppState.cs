using System.Windows.Controls;
using trizbd.Classes;
using trizbd.Pages;

namespace trizbd;

/// <summary>
/// Простое глобальное состояние приложения (курсовой проект):
/// текущий сотрудник + ссылка на основной Frame.
/// </summary>
public static class AppState
{
    public static Employee? CurrentEmployee { get; set; }

    /// <summary>
    /// Frame внутри страницы Main, куда навигируемся между разделами.
    /// </summary>
    public static Frame? MainFrame { get; set; }

    /// <summary>
    /// Ссылка на страницу Main (нужно, чтобы из внутренних страниц можно было
    /// корректно переключать разделы и подсветку меню).
    /// </summary>
    public static Main? MainPage { get; set; }

    /// <summary>
    /// Если из расписания/деталей фильма переходим в продажу билетов, сюда
    /// кладём выбранный SessionId, чтобы Ticket_Sale сразу подхватил нужный сеанс.
    /// </summary>
    public static int? PendingSessionId { get; set; }

    /// <summary>
    /// Последний выбранный MovieId (для перехода в детали).
    /// </summary>
    public static int? PendingMovieId { get; set; }
}
