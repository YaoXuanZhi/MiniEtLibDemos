using MongoDB.Bson.Serialization.Attributes;

namespace ET.Server
{
    [ChildOf(typeof(PlayerComponent))]
    public sealed class Player : Entity, IAwake<string>
    {
        public string Account { get; set; }
		
        public long UnitId { get; set; }
        
        [BsonIgnore]
        public long UserId => Id;
        [BsonIgnore]
        public bool IsOnline { get; set; }
    }
}