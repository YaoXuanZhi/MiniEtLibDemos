namespace ET.Server
{
    public class GmHandlerAttribute: BaseAttribute
    {
        public string GmName { get; }
        public string GmComment { get; }
        
        public GmHandlerAttribute(string gmName, string gmComment
        )
        {
            this.GmName = gmName;
            this.GmComment = gmComment;
        }
    }
}