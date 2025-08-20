using MessagePack;

namespace DH.SignalR.Dtos;

[MessagePackObject(keyAsPropertyName: true)]
public class NotifyConnectsData
{
    /// <summary>
    /// 用户标识
    /// </summary>
    public String UserId { set; get; }

    /// <summary>
    /// 连接Id
    /// </summary>
    public String ConnectionId { set; get; }

    /// <summary>
    /// 是否排除指定连接,当且仅当UserId有值的情况才有效
    /// </summary>
    public Boolean ExcludeConnectId { set; get; }

    /// <summary>
    /// 类型
    /// </summary>
    public String TenantType { get; set; }

    /// <summary>
    /// 对象内容
    /// </summary>
    public virtual Object NotifyObj { set; get; }
}