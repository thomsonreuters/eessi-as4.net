namespace Eu.EDelivery.AS4.Fe.Monitor.Model
{
    public enum DateTimeFilterType
    {
        Ignore = -1,
        LastHour = 0,
        Last4Hours = 1,
        LastDay = 2,
        LastWeek = 3,
        LastMonth = 4,
        Custom = 5
    }
}