namespace TicketSystemApi.Dtos;

public class OrderUpdateDto
{
    public int Id { get; set; }
    public int Quantity { get; set; }

    /* 注意前提
        若你這樣寫：
        public int Quantity { get; set; } // 非 nullable
        那就算前端沒傳值，C# 會給你 0，這樣 srcMember != null 仍會是 true。
        所以：如果你真要跳過未傳欄位，建議 DTO 的屬性用 nullable：
        public int? Quantity { get; set; }
    */
}
