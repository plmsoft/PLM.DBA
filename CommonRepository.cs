using PLM.DBA.Core.Common;

namespace PLM.DBA
{
    public class CommonRepository
    {
        public CommonRepository(string connectionString)
        {
            Command = new DBACommand(connectionString);
            Transaction = new TransactionProvider();
        }

        protected IDBACommand Command { get; }
        protected ITransactionProvider Transaction { get; }
    }
}
