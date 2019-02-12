using System;
using HashidsNet;

namespace SFA.DAS.Reservations.Application.Reservations.Services
{
    public class HashingService : IHashingService
    {
        private readonly IHashids _hashIds;

        public HashingService(IHashids hashIds)
        {
            _hashIds = hashIds;
        }

        public string HashValue(long id)
        {
            return _hashIds.EncodeLong(id);
        }

        public long DecodeValue(string id)
        {
            ValidateInput(id);
            return _hashIds.DecodeLong(id)[0];
        }

        private void ValidateInput(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Invalid hash Id", nameof(id));
        }
    }
}