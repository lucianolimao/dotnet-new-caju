﻿namespace MyProject.Application.Commands.Deposit
{
    using System.Threading.Tasks;
    using MyProject.Application.Results;
    using MyProject.Domain.ValueObjects;
    using MyProject.Application.Repositories;
    using MyProject.Domain.Accounts;

    public class DepositService : IDepositService
    {
        private readonly IAccountReadOnlyRepository accountReadOnlyRepository;
        private readonly IAccountWriteOnlyRepository accountWriteOnlyRepository;
        private readonly IResultConverter resultConverter;

        public DepositService(
            IAccountReadOnlyRepository accountReadOnlyRepository,
            IAccountWriteOnlyRepository accountWriteOnlyRepository,
            IResultConverter resultConverter)
        {
            this.accountReadOnlyRepository = accountReadOnlyRepository;
            this.accountWriteOnlyRepository = accountWriteOnlyRepository;
            this.resultConverter = resultConverter;
        }

        public async Task<DepositResult> Process(DepositCommand command)
        {
            Account account = await accountReadOnlyRepository.Get(command.AccountId);
            if (account == null)
                throw new AccountNotFoundException($"The account {command.AccountId} does not exists or is already closed.");

            Credit credit = new Credit(account.Id, command.Amount);
            account.Deposit(credit);

            await accountWriteOnlyRepository.Update(account, credit);

            TransactionResult transactionResult = resultConverter.Map<TransactionResult>(credit);
            DepositResult result = new DepositResult(transactionResult, account.GetCurrentBalance().Value);

            return result;
        }
    }
}
