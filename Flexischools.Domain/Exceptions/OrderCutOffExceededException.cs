namespace Flexischools.Domain.Exceptions;

public class OrderCutOffExceededException : Exception
{
    public DateTime CutOffTime { get; }
    public DateTime RequestedTime { get; }
    
    public OrderCutOffExceededException(DateTime cutOffTime, DateTime requestedTime)
        : base($"Order cut-off time ({cutOffTime:HH:mm}) has been exceeded. Requested at {requestedTime:HH:mm}")
    {
        CutOffTime = cutOffTime;
        RequestedTime = requestedTime;
    }
}
