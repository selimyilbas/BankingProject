using AutoMapper;
using BankingApp.Domain.Entities;
using BankingApp.Application.DTOs.Customer;
using BankingApp.Application.DTOs.Account;
using BankingApp.Application.DTOs.Transaction;
using BankingApp.Application.DTOs.Transfer;

namespace BankingApp.Application.Mappings
{
    /// <summary>
    /// Domain varlıkları ile DTO'lar arasındaki eşlemeleri tanımlar.
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Eşleme konfigurasyonlarını başlatır.
        /// </summary>
        public MappingProfile()
        {
            /// <summary>
            /// Customer eşlemeleri
            /// </summary>
            CreateMap<Customer, CustomerDto>()
                .ForMember(dest => dest.Accounts, opt => opt.MapFrom(src => src.Accounts));
            
            CreateMap<Customer, CustomerWithAccountsDto>()
                .ForMember(dest => dest.Accounts, opt => opt.MapFrom(src => src.Accounts));
            
            CreateMap<Customer, CustomerSummaryDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
            
            CreateMap<CreateCustomerDto, Customer>();
            CreateMap<UpdateCustomerDto, Customer>();

            /// <summary>
            /// Account eşlemeleri
            /// </summary>
            CreateMap<Account, AccountDto>()
                .ForMember(dest => dest.CustomerName, 
                    opt => opt.MapFrom(src => src.Customer != null ? $"{src.Customer.FirstName} {src.Customer.LastName}" : string.Empty));
            
            CreateMap<Account, AccountBalanceDto>();
            
            CreateMap<CreateAccountDto, Account>();

            /// <summary>
            /// Transaction eşlemeleri
            /// </summary>
            CreateMap<Transaction, TransactionDto>()
                .ForMember(dest => dest.AccountNumber, opt => opt.MapFrom(src => src.Account != null ? src.Account.AccountNumber : string.Empty));
            
            CreateMap<DepositDto, Transaction>();

            /// <summary>
            /// Transfer eşlemeleri
            /// </summary>
            CreateMap<Transfer, TransferDto>()
                .ForMember(dest => dest.FromAccountNumber, opt => opt.MapFrom(src => src.FromAccount != null ? src.FromAccount.AccountNumber : string.Empty))
                .ForMember(dest => dest.ToAccountNumber, opt => opt.MapFrom(src => src.ToAccount != null ? src.ToAccount.AccountNumber : string.Empty));
            
            CreateMap<CreateTransferDto, Transfer>();
        }
    }
}
