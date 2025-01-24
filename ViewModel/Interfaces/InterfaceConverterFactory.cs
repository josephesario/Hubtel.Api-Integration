using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using ViewModel.Data;

namespace ViewModel.Interfaces
{
    public class InterfaceConverterFactory : JsonConverterFactory
    {
        private static readonly Dictionary<Type, Type> _interfaceMappings = new()
    {
        { typeof(IUserType), typeof(UserType) },
        { typeof(ICardType), typeof(CardType) },
        { typeof(ISimcardType), typeof(SimcardType) },
        { typeof(IUserAccess), typeof(UserAccess) },
        { typeof(IUserProfile), typeof(UserProfile) },
        { typeof(ICardAccountDetail), typeof(CardAccountDetail) },
        { typeof(ICardAccountDetailOut), typeof(CardAccountDetailOut) },
        { typeof(IPhoneAccountDetail), typeof(PhoneAccountDetail) },
        { typeof(IPhoneAccountDetailOut), typeof(PhoneAccountDetailOut) }
    };

        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsInterface && _interfaceMappings.ContainsKey(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var concreteType = _interfaceMappings[typeToConvert];
            return (JsonConverter)Activator.CreateInstance(
                typeof(InterfaceConverter<,>).MakeGenericType(typeToConvert, concreteType)
            )!;
        }
    }

    public class InterfaceConverter<TInterface, TImplementation> : JsonConverter<TInterface>
        where TImplementation : TInterface, new()
    {
        public override TInterface? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<TImplementation>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, typeof(TImplementation), options);
        }
    }

}
