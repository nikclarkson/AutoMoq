using System;

namespace Orders
{
    public interface IAddressValidator
    {
        bool ValidateAddress(string address);
    }

    public class AddressValidator : IAddressValidator
    {
        public bool ValidateAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new Exception("addres is invalid");
            }

            return true;
        }
    }
}