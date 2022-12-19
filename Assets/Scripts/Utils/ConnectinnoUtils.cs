using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace ConnectinnoGames.Utils
{
    public static class ConnectinnoUtils
    {
        /// <summary>
        /// Async Task Awaitrer Method
        /// </summary>
        /// <param name="asyncOp"></param>
        /// <returns></returns>
        public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<object>();
            asyncOp.completed += obj => { tcs.SetResult(null); };
            return ((Task)tcs.Task).GetAwaiter();
        }
    }

}
