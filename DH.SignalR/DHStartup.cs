﻿using Pek.Infrastructure;
using Pek.VirtualFileSystem;

namespace DH.SignalR;

/// <summary>
/// 表示应用程序启动时配置SignalR的对象
/// </summary>
public class DHStartup : IPekStartup
{
    /// <summary>
    /// 配置添加的中间件的使用
    /// </summary>
    /// <param name="application">用于配置应用程序的请求管道的生成器</param>
    public void Configure(IApplicationBuilder application)
    {
    }

    /// <summary>
    /// 添加并配置任何中间件
    /// </summary>
    /// <param name="services">服务描述符集合</param>
    /// <param name="configuration">应用程序的配置</param>
    /// <param name="webHostEnvironment">应用程序的环境</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        if (SignalRSetting.Current.IsAllowSignalR)
        {
            // 添加SignalR
            services.AddSignalR(config =>
            {
                // webHostEnvironment为通过依赖注入在Startup的构造函数中注入的 IWebHostEnvironment
                if (webHostEnvironment.IsDevelopment())
                {
                    config.EnableDetailedErrors = true;
                }
                config.MaximumParallelInvocationsPerClient = 10; // 每个客户端可以在进行排队之前并行调用的最大集线器方法数
            }) // 支持MessagePack
                .AddMessagePackProtocol()
                .AddJsonProtocol();
        }
    }

    /// <summary>
    /// 配置虚拟文件系统
    /// </summary>
    /// <param name="options">虚拟文件配置</param>
    public void ConfigureVirtualFileSystem(DHVirtualFileSystemOptions options)
    {
        options.FileSets.AddEmbedded<NotifyHub>(typeof(NotifyHub).Namespace);
        // options.FileSets.Add(new EmbeddedFileSet(item.Assembly, item.Namespace));
    }

    /// <summary>
    /// 注册路由
    /// </summary>
    /// <param name="endpoints">路由生成器</param>
    public void UseDHEndpoints(IEndpointRouteBuilder endpoints)
    {
        if (SignalRSetting.Current.IsAllowSignalR)
        {
            endpoints.MapHub<NotifyHub>("/notify-hub");
        }
    }

    /// <summary>
    /// 将区域路由写入数据库
    /// </summary>
    public void ConfigureArea()
    {

    }

    /// <summary>
    /// 调整菜单
    /// </summary>
    public void ChangeMenu()
    {

    }

    /// <summary>
    /// 升级处理逻辑
    /// </summary>
    public void Update()
    {

    }

    /// <summary>
    /// 配置使用添加的中间件
    /// </summary>
    /// <param name="application">用于配置应用程序的请求管道的生成器</param>
    public void ConfigureMiddleware(IApplicationBuilder application)
    {
        if (SignalRSetting.Current.IsAllowSignalR)
        {
            application.UseMiddleware<SignalRMiddleware>();  // 将access_token加到标头
        }
    }

    /// <summary>
    /// UseRouting前执行的数据
    /// </summary>
    /// <param name="application"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void BeforeRouting(IApplicationBuilder application)
    {

    }

    /// <summary>
    /// UseAuthentication或者UseAuthorization后面 Endpoints前执行的数据
    /// </summary>
    /// <param name="application"></param>
    public void AfterAuth(IApplicationBuilder application)
    {

    }

    /// <summary>
    /// 处理数据
    /// </summary>
    public void ProcessData()
    {
       
    }

    /// <summary>
    /// 获取此启动配置实现的顺序
    /// </summary>
    public int StartupOrder => 450;

    /// <summary>
    /// 获取此启动配置实现的顺序。主要针对ConfigureMiddleware、UseRouting前执行的数据、UseAuthentication或者UseAuthorization后面 Endpoints前执行的数据
    /// </summary>
    public Int32 ConfigureOrder => 0;
}
