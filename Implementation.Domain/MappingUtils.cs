using System.Collections.Generic;
using AutoMapper;
using Integration.DataAccess.Entitys;
using Integration.Domain.DataTransferObjects;

namespace Implementation.Domain
{
    public static class MappingUtils
    {
        private static bool IsMappingInitialized { get; set; }

        public static TTarget Map<TTarget>(object source)
        {
            InitializeMapper();

            return Mapper.Map<TTarget>(source);
        }

        public static TTarget Map<TTarget>(object source, object target) where TTarget : class
        {
            InitializeMapper();

            return Mapper.Map(source, target) as TTarget;
        }

        public static IEnumerable<TTarget> MapRange<TTarget>(IEnumerable<object> sources)
        {
            InitializeMapper();

            var mappedRange = new LinkedList<TTarget>();

            foreach (var source in sources)
            {
                mappedRange.AddLast(Map<TTarget>(source));
            }

            return mappedRange;
        }

        private static void InitializeMapper()
        {
            if (!IsMappingInitialized)
            {
                Mapper.Initialize(expression => { expression?.CreateMap<RequestJob, NewRequestJobDto>()?.ReverseMap(); });
                Mapper.Initialize(expression => { expression?.CreateMap<RequestJob, RequestJob>()?.ReverseMap(); });
                Mapper.Initialize(expression => { expression?.CreateMap<RequestHeader, RequestHeader>()?.ReverseMap(); });
                Mapper.Initialize(expression => { expression?.CreateMap<ResponseMessage, ResponseMessage>()?.ReverseMap(); });
                Mapper.Initialize(expression => { expression?.CreateMap<ResponseHeader, ResponseHeader>()?.ReverseMap(); });
            }

            IsMappingInitialized = true;
        }
    }
}