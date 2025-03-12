using MessagePack;

namespace DH.SignalR.Dtos;

/// <summary>
/// 上线数据
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class OnLineData
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public Int64 UserId { set; get; }

    /// <summary>
    /// 连接Id
    /// </summary>
    public String ConnectionId { set; get; }

    /// <summary>
    /// 是否该用户的最后一个连接
    /// </summary>
    public Boolean IsFirst { set; get; }

    /// <summary>
    /// 页面打开时的惟一标识，如果是刷新或者其他页面则变动，用于区分主体
    /// </summary>
    public Int64 PageRnd { set; get; }
}