﻿using System.Security.Claims;

using DH.Permissions;
using DH.SignalR.Dtos;

using Microsoft.AspNetCore.SignalR;

using NewLife;
using NewLife.Caching;
using NewLife.Log;

using Pek;
using Pek.Configs;
using Pek.Helpers;

namespace DH.SignalR;

/// <summary>
/// 服务端接口
/// </summary>
public interface IServerNotifyHub
{
}

/// <summary>
/// 客户端使用的接口
/// </summary>
public interface IClientNotifyHub
{
    /// <summary>
    /// 统一的客户端通知方法
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    Task OnNotify(Object data);

    /// <summary>
    /// 在线
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    Task OnLine(Object data);

    /// <summary>
    /// 离线
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    Task OffLine(Object data);
}

[JwtAuthorize("Web")]
public class NotifyHub : Hub<IClientNotifyHub>, IServerNotifyHub
{
    /// <summary>
    /// 缓存
    /// </summary>
    private readonly ICache _cache;

    public NotifyHub(ICacheProvider cache)
    {
        _cache ??= cache.Cache;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = DHWeb.Identity.GetValue(ClaimTypes.Sid).ToInt();
        var dgpage = Context.GetHttpContext().Request.Query["dgpage"].FirstOrDefault();
        var iotid = Context.GetHttpContext().Request.Query["iotid"].FirstOrDefault();
        var pageRnd = Context.GetHttpContext().Request.Query["pageRnd"].FirstOrDefault().ToLong();

#if DEBUG
        XTrace.WriteLine($"[NotifyHub.OnConnectedAsync]OnConnectedAsync----userId:{userId},dgpage:{dgpage},iotid:{iotid},connectionId:{Context.ConnectionId}");
#endif

        if (userId != 0)
        {
            _cache.Increment($"{SignalRSetting.Current.SignalRPrefixUser}{RedisSetting.Current.CacheKeyPrefix}{userId}Count", 1);
            await JoinToGroup(userId, Context.ConnectionId, dgpage, iotid).ConfigureAwait(false);
            await DealOnLineNotify(userId, Context.ConnectionId, pageRnd).ConfigureAwait(false);
        }

        await base.OnConnectedAsync().ConfigureAwait(false);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = DHWeb.Identity.GetValue(ClaimTypes.Sid).ToInt();
        var dgpage = Context.GetHttpContext().Request.Query["dgpage"].FirstOrDefault();
        var iotid = Context.GetHttpContext().Request.Query["iotid"].FirstOrDefault();

#if DEBUG
        XTrace.WriteLine($"[NotifyHub.OnDisconnectedAsync]OnDisconnectedAsync----userId:{userId},dgpage:{dgpage},iotid:{iotid},connectionId:{Context.ConnectionId}");
#endif

        if (userId != 0)
        {
            _cache.Decrement($"{SignalRSetting.Current.SignalRPrefixUser}{RedisSetting.Current.CacheKeyPrefix}{userId}Count", 1);
            await DealOffLineNotify(userId, Context.ConnectionId).ConfigureAwait(false);
        }

        await LeaveFromGroup(Context.ConnectionId, dgpage, iotid).ConfigureAwait(false);
        await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
    }

    public async Task SendAll(Object data)
    {
        await Clients.All.OnNotify(data).ConfigureAwait(false);
    }

    public async Task Send(Object data, String Group)
    {
        await Clients.Group(Group).OnNotify(data).ConfigureAwait(false);
    }

    /// <summary>
    /// 处理上线通知(只有用户第一个连接才通知)
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <param name="connectionId">连接Id</param>
    /// <param name="pageRnd">页面打开时的惟一标识，如果是刷新或者其他页面则变动，用于区分主体</param>
    /// <returns></returns>
    private async Task DealOnLineNotify(Int32 userId, String connectionId, Int64 pageRnd)
    {
        var userConnectCount = _cache.Get<Int32>($"{SignalRSetting.Current.SignalRPrefixUser}{RedisSetting.Current.CacheKeyPrefix}{userId}Count");
        await Clients.All.OnLine(new OnLineData
        {
            UserId = userId,
            ConnectionId = connectionId,
            PageRnd = pageRnd,
            IsFirst = userConnectCount == 1
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// 处理下线通知(只有当用户一个连接都没了 才算下线)
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <param name="connectionId">连接Id</param>
    /// <returns></returns>
    private async Task DealOffLineNotify(Int32 userId, String connectionId)
    {
        var userConnectCount = _cache.Get<Int32>($"{SignalRSetting.Current.SignalRPrefixUser}{RedisSetting.Current.CacheKeyPrefix}{userId}Count");
        await Clients.All.OffLine(new OffLineData
        {
            UserId = userId,
            ConnectionId = connectionId,
            IsLast = userConnectCount == 0
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// 加入组
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <param name="connectionId">连接Id</param>
    /// <param name="groups">组</param>
    /// <returns></returns>
    private async Task JoinToGroup(Int32 userId, String connectionId, params String[] groups)
    {
        if (userId > 0 && groups != null && groups.Length > 0)
        {
            foreach (var group in groups)
            {
                if (!group.IsNullOrWhiteSpace())
                {
                    await Groups.AddToGroupAsync(connectionId, group).ConfigureAwait(false);

                    var dic = _cache.GetDictionary<Int32>($"{SignalRSetting.Current.SignalRPrefixGroup}{RedisSetting.Current.CacheKeyPrefix}{group}");
                    dic.Add(connectionId, userId);
                }
            }
        }
    }

    /// <summary>
    /// 从组中移除
    /// </summary>
    /// <param name="connectionId">连接Id</param>
    /// <param name="groups">组</param>
    /// <returns></returns>
    private async Task LeaveFromGroup(String connectionId, params String[] groups)
    {
        if (groups != null && groups.Length > 0)
        {
            foreach (var group in groups)
            {
                if (!group.IsNullOrWhiteSpace())
                {
                    await Groups.RemoveFromGroupAsync(connectionId, group).ConfigureAwait(false);

                    var dic = _cache.GetDictionary<Int32>($"{SignalRSetting.Current.SignalRPrefixGroup}{RedisSetting.Current.CacheKeyPrefix}{group}");
                    dic.Remove(connectionId);
                }
            }
        }
    }
}