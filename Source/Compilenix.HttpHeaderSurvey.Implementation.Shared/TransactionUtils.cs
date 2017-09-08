using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Transactions;

namespace Compilenix.HttpHeaderSurvey.Implementation.Shared
{
    [DebuggerStepThrough]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Utils")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public static class TransactionUtils
    {
        public static TimeSpan DefaultTransactionTimeOut { get; set; } = TimeSpan.FromSeconds(30);

        public static TransactionScope CreateNewTransactionScope()
        {
            return new TransactionScope(TransactionScopeOption.Required, CreateTransactionOptions(DefaultTransactionTimeOut));
        }

        public static TransactionScope CreateNewTransactionScope(TimeSpan timeout)
        {
            return new TransactionScope(TransactionScopeOption.Required, CreateTransactionOptions(timeout));
        }

        public static TransactionScope CreateNewTransactionScope(TransactionScopeOption scopeOption)
        {
            return new TransactionScope(scopeOption, CreateTransactionOptions(DefaultTransactionTimeOut));
        }

        public static TransactionScope CreateNewTransactionScope(TransactionScopeOption scopeOption, TimeSpan timeout)
        {
            return new TransactionScope(scopeOption, CreateTransactionOptions(timeout));
        }

        public static TransactionScope CreateNewTransactionScope(TransactionScopeOption scopeOption, TransactionOptions transactionOptions)
        {
            return new TransactionScope(scopeOption, transactionOptions);
        }

        public static TransactionOptions CreateTransactionOptions(TimeSpan timeout)
        {
            return new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = timeout };
        }
    }
}