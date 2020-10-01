using System.Transactions;

namespace PLM.DBA.Core.Common
{
	public interface ITransactionProvider
	{
		TransactionScope TransactionStart();
		void TransactionCommit();
		void TransactionRollback();
	}
}
