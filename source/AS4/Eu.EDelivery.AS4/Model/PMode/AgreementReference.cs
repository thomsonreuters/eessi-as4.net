namespace Eu.EDelivery.AS4.Model.PMode
{
    public class AgreementReference 
    {
        public string Value { get; set; }
        public string Type { get; set; }
        public string PModeId { get; set; }

        /// <summary>
        /// Indicates wheter the curent Agreement Ref is empty
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return
                string.IsNullOrEmpty(this.Value) &&
                string.IsNullOrEmpty(this.Type) &&
                string.IsNullOrEmpty(this.PModeId);
        }
    }
}