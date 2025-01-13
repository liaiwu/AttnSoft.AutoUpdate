using System.Threading.Tasks;
using UpgradExpress.Objects;

namespace Upgrad.Strategys
{
    /// <summary>
    /// Update the strategy, if you need to extend it, you need to inherit this interface.
    /// </summary>
    internal interface IStrategy
    {
        /// <summary>
        /// Execution strategy.
        /// </summary>
        Task Execute();

        /// <summary>
        /// After the update is complete.
        /// </summary>
        void StartApp();
        
        /// <summary>
        /// Execution strategy.
        /// </summary>
        Task ExecuteAsync();

        /// <summary>
        /// Create a strategy.
        /// </summary>
        void Create(UpgradContext content);
    }
}