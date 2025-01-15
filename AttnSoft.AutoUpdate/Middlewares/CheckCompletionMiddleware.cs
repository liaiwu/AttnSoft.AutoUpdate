using System;
using System.Threading.Tasks;

namespace AttnSoft.AutoUpdate.Middlewares;
/// <summary>
/// 检查升级是否成功
/// </summary>
public interface ICheckCompletion : IApplicationMiddleware<UpdateContext> { }
/// <summary>
/// 检查升级是否成功中间件
/// </summary>
public class CheckCompletionMiddleware : ICheckCompletion
{
    public async Task InvokeAsync(ApplicationDelegate<UpdateContext> next, UpdateContext context)
    {
        if (CheckIsCompletion() && context.OnUpdateCompleted!=null)
        {
            context.OnUpdateCompleted.Invoke(context);
        }
        await next(context);
    }
    public static bool CheckIsCompletion()
    {
        string[] args = Environment.GetCommandLineArgs();
        for (var index = 0; index < args.Length; index++)
        {
            string arg = args[index];
            //Console.WriteLine(arg);
            if (arg== "AttnSoft.AutoUpdate.Successful")return true;
        }
        return false;
    }
}