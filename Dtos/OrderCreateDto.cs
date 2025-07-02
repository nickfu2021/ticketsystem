namespace TicketSystemApi.Dtos;

public class OrderCreateDto
{
    public int EventId { get; set; }
    public int CustomerId { get; set; }
    public int Quantity { get; set; }

    // 可以考慮加入其他欄位，例如付款方式、訂單狀態等
}