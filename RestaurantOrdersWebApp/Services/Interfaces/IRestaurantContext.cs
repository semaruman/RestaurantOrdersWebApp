namespace RestaurantOrdersWebApp.Services.Interfaces
{
    /* 
     * Сервис для работы с рестораном, который содержит его имя.
     * Когда клиент отправляет запрос - он отправляет его для каког-то ресторана с 
     * каким-то названием. Это название сохраняется в Scoped сервис и 
     * RestaurantMiddleware получает имя через сервис
    */
    public interface IRestaurantContext
    {
        string RestaurantName { get; set; }
    }
}
