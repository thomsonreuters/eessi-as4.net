using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Notify
{
    /// <summary>
    /// Map a corresponding <see cref="Model.Core.Receipt"/> with <see cref="Model.Notify.NotifyMessage"/>
    /// </summary>
    public class ReceiptToNotifyMap : Profile
    {
        public ReceiptToNotifyMap()
        {
            CreateMap<Model.Core.Receipt, Model.Notify.NotifyMessage>()
                .AfterMap((receipt, notifyMessage) =>
                {
                    notifyMessage.MessageInfo.MessageId = receipt.MessageId;
                    notifyMessage.MessageInfo.RefToMessageId = receipt.RefToMessageId;
                    notifyMessage.StatusInfo.Status = Model.Notify.Status.Delivered;
                })
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}
