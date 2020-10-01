using System;
using System.Transactions;

namespace PLM.DBA.Core.Common
{
	public class TransactionProvider : ITransactionProvider, IDisposable
	{
		private TransactionScope _transaction { get; set; }

		public void TransactionCommit()
		{
			if (_transaction != null)
				this.Dispose();
		}

		public void TransactionRollback()
		{
			if (_transaction != null)
			{
				_transaction.Dispose();
				_transaction = null;
			}
		}

		public TransactionScope TransactionStart()
		{
			if (_transaction != null)
				return _transaction;
			return _transaction = new TransactionScope(TransactionScopeOption.RequiresNew);
		}

		public void Dispose()
		{
			if (_transaction != null)
			{
				_transaction.Complete();
				_transaction.Dispose();
				_transaction = null;
			}
		}

	}
}
