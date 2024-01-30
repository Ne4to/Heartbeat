using Heartbeat.Host.Endpoints;

using JWT;
using JWT.Serializers.Converters;

using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Heartbeat.Host.Helpers;

internal static class JwtParser
{
    private static readonly IReadOnlySet<string> _numericDateClaimKeys = new HashSet<string>() { "exp", "nbf", "iat" }.ToFrozenSet();

    // https://datatracker.ietf.org/doc/html/rfc7515#section-4.1
    private static readonly IReadOnlyDictionary<string, string> _headerKeyDescription = new Dictionary<string, string>
    {
        ["alg"] = "Algorithm",
        ["jku"] = "JWK Set URL",
        ["jwk"] = "JSON Web Key",
        ["kid"] = "Key ID",
        ["x5u"] = "X.509 URL",
        ["x5c"] = "X.509 Certificate Chain",
        ["x5t"] = "X.509 Certificate SHA-1 Thumbprint",
        ["x5t#S256"] = "X.509 Certificate SHA-256 Thumbprint",
        ["typ"] = "Type",
        ["cty"] = "Content Type",
        ["crit"] = "Critical",
    }.ToFrozenDictionary();

    // https://www.iana.org/assignments/jwt/jwt.xhtml
    private static readonly IReadOnlyDictionary<string, string> _payloadKeyDescription = new Dictionary<string, string>
    {
        ["iss"] = "Issuer",
        ["sub"] = "Subject",
        ["aud"] = "Audience",
        ["exp"] = "Expiration Time",
        ["nbf"] = "Not Before",
        ["iat"] = "Issued At",
        ["jti"] = "JWT ID",
        ["name"] = "Full name",
        ["given_name"] = "Given name(s) or first name(s)",
        ["family_name"] = "Surname(s) or last name(s)",
        ["middle_name"] = "Middle name(s)",
        ["nickname"] = "Casual name",
        ["preferred_username"] = "Shorthand name by which the End-User wishes to be referred to",
        ["profile"] = "Profile page URL",
        ["picture"] = "Profile picture URL",
        ["website"] = "Web page or blog URL",
        ["email"] = "Preferred e-mail address",
        ["email_verified"] = "True if the e-mail address has been verified; otherwise false",
        ["gender"] = "Gender",
        ["birthdate"] = "Birthday",
        ["zoneinfo"] = "Time zone",
        ["locale"] = "Locale",
        ["phone_number"] = "Preferred telephone number",
        ["phone_number_verified"] = "True if the phone number has been verified; otherwise false",
        ["address"] = "Preferred postal address",
        ["updated_at"] = "Time the information was last updated",
        ["azp"] = "Authorized party - the party to which the ID Token was issued",
        ["nonce"] = "Value used to associate a Client session with an ID Token (MAY also be used for nonce values in other applications of JWTs)",
        ["auth_time"] = "Time when the authentication occurred",
        ["at_hash"] = "Access Token hash value",
        ["c_hash"] = "Code hash value",
        ["acr"] = "Authentication Context Class Reference",
        ["amr"] = "Authentication Methods References",
        ["sub_jwk"] = "Public key used to check the signature of an ID Token",
        ["cnf"] = "Confirmation",
        ["sip_from_tag"] = "SIP From tag header field parameter value",
        ["sip_date"] = "SIP Date header field value",
        ["sip_callid"] = "SIP Call-Id header field value",
        ["sip_cseq_num"] = "SIP CSeq numeric header field parameter value",
        ["sip_via_branch"] = "SIP Via branch header field parameter value",
        ["orig"] = "Originating Identity String",
        ["dest"] = "Destination Identity String",
        ["mky"] = "Media Key Fingerprint String",
        ["events"] = "Security Events",
        ["toe"] = "Time of Event",
        ["txn"] = "Transaction Identifier",
        ["rph"] = "Resource Priority Header Authorization",
        ["sid"] = "Session ID",
        ["vot"] = "Vector of Trust value",
        ["vtm"] = "Vector of Trust trustmark URL",
        ["attest"] = "Attestation level as defined in SHAKEN framework",
        ["origid"] = "Originating Identifier as defined in SHAKEN framework",
        ["act"] = "Actor",
        ["scope"] = "Scope Values",
        ["client_id"] = "Client Identifier",
        ["may_act"] = "Authorized Actor - the party that is authorized to become the actor",
        ["jcard"] = "jCard data",
        ["at_use_nbr"] = "Number of API requests for which the access token can be used",
        ["div"] = "Diverted Target of a Call",
        ["opt"] = "Original PASSporT (in Full Form)",
        ["vc"] = "Verifiable Credential as specified in the W3C Recommendation",
        ["vp"] = "Verifiable Presentation as specified in the W3C Recommendation",
        ["sph"] = "SIP Priority header field",
        ["ace_profile"] = "The ACE profile a token is supposed to be used with.",
        ["cnonce"] =
            "\"client-nonce\". A nonce previously provided to the AS by the RS via the client. Used to verify token freshness when the RS cannot synchronize its clock with the AS.",
        ["exi"] =
            "\"Expires in\". Lifetime of the token in seconds from the time the RS first sees it. Used to implement a weaker from of token expiration for devices that cannot synchronize their internal clocks.",
        ["roles"] = "Roles",
        ["groups"] = "Groups",
        ["entitlements"] = "Entitlements",
        ["token_introspection"] = "Token introspection response",
        ["eat_nonce"] = "Nonce",
        ["ueid"] = "The Universal Entity ID",
        ["sueids"] = "Semi-permanent UEIDs",
        ["oemid"] = "Hardware OEM ID",
        ["hwmodel"] = "Model identifier for hardware",
        ["hwversion"] = "Hardware Version Identifier",
        ["oemboot"] = "Indicates whether the software booted was OEM authorized",
        ["dbgstat"] = "Indicates status of debug facilities",
        ["location"] = "The geographic location",
        ["eat_profile"] = "Indicates the EAT profile followed",
        ["submods"] = "The section containing submodules",
        ["uptime"] = "Uptime",
        ["bootcount"] = "The number times the entity or submodule has been booted",
        ["bootseed"] = "Identifies a boot cycle",
        ["dloas"] = "Certifications received as Digital Letters of Approval",
        ["swname"] = "The name of the software running in the entity",
        ["swversion"] = "The version of software running in the entity",
        ["manifests"] = "Manifests describing the software installed on the entity",
        ["measurements"] = "Measurements of the software, memory configuration and such on the entity",
        ["measres"] = "The results of comparing software measurements to reference values",
        ["intuse"] = "Indicates intended use of the EAT",
        ["cdniv"] = "CDNI Claim Set Version",
        ["cdnicrit"] = "CDNI Critical Claims Set",
        ["cdniip"] = "CDNI IP Address",
        ["cdniuc"] = "CDNI URI Container",
        ["cdniets"] = "CDNI Expiration Time Setting for Signed Token Renewal",
        ["cdnistt"] = "CDNI Signed Token Transport Method for Signed Token Renewal",
        ["cdnistd"] = "CDNI Signed Token Depth",
        ["sig_val_claims"] = "Signature Validation Token",
        ["authorization_details"] =
            "The claim authorization_details contains a JSON array of JSON objects representing the rights of the access token. Each JSON object contains the data to specify the authorization requirements for a certain type of resource.",
        ["verified_claims"] =
            "This container Claim is composed of the verification evidence related to a certain verification process and the corresponding Claims about the End-User which were verified in this process.",
        ["place_of_birth"] = "A structured Claim representing the End-User's place of birth.",
        ["nationalities"] = "String array representing the End-User's nationalities.",
        ["birth_family_name"] =
            "Family name(s) someone has when they were born, or at least from the time they were a child. This term can be used by a person who changes the family name(s) later in life for any reason. Note that in some cultures, people can have multiple family names or no family name; all can be present, with the names being separated by space characters.",
        ["birth_given_name"] =
            "Given name(s) someone has when they were born, or at least from the time they were a child. This term can be used by a person who changes the given name later in life for any reason. Note that in some cultures, people can have multiple given names; all can be present, with the names being separated by space characters.",
        ["birth_middle_name"] =
            "Middle name(s) someone has when they were born, or at least from the time they were a child. This term can be used by a person who changes the middle name later in life for any reason. Note that in some cultures, people can have multiple middle names; all can be present, with the names being separated by space characters. Also note that in some cultures, middle names are not used.",
        ["salutation"] = "End-User's salutation, e.g., \"Mr.\"",
        ["title"] = "End-User's title, e.g., \"Dr.\"",
        ["msisdn"] = "End-User's mobile phone number formatted according to ITU-T recommendation [E.164]",
        ["also_known_as"] =
            "Stage name, religious name or any other type of alias/pseudonym with which a person is known in a specific context besides its legal name. This must be part of the applicable legislation and thus the trust framework (e.g., be an attribute on the identity card).",
        ["htm"] = "The HTTP method of the request",
        ["htu"] = "The HTTP URI of the request (without query and fragment parts)",
        ["ath"] = "The base64url-encoded SHA-256 hash of the ASCII encoding of the associated access token's value",
        ["atc"] = "Authority Token Challenge",
        ["sub_id"] = "Subject Identifier",
        ["rcd"] = "Rich Call Data Information",
        ["rcdi"] = "Rich Call Data Integrity Information",
        ["crn"] = "Call Reason",
        ["msgi"] = "Message Integrity Information",
        ["_claim_names"] = "JSON object whose member names are the Claim Names for the Aggregated and Distributed Claims",
        ["_claim_sources"] = "JSON object whose member names are referenced by the member values of the _claim_names member",
        ["rdap_allowed_purposes"] =
            "This claim describes the set of RDAP query purposes that are available to an identity that is presented for access to a protected RDAP resource.",
        ["rdap_dnt_allowed"] =
            "This claim contains a JSON boolean literal that describes a \"do not track\" request for server-side tracking, logging, or recording of an identity that is presented for access to a protected RDAP resource.",
    }.ToFrozenDictionary();

    public static JwtInfo ToJwtInfo(string str)
    {
        if (str.StartsWith("Bearer "))
        {
            str = str["Bearer ".Length..];
        }

        IJsonSerializer serializer = new AotSystemTextSerializer();
        IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        IJwtDecoder decoder = new JwtDecoder(serializer, urlEncoder);

        var header = decoder.DecodeHeaderToDictionary(str);
        var payload = decoder.DecodeToObject(str, false);
        var headerValues = header
            .Select(kvp => new JwtValue(kvp.Key, kvp.Value, _headerKeyDescription.GetValueOrDefault(kvp.Key)))
            .ToArray();

        var payloadValues = payload
            .Select(kvp => new JwtValue(
                kvp.Key,
                _numericDateClaimKeys.Contains(kvp.Key)
                    ? $"{kvp.Value} ({DateTimeOffset.FromUnixTimeSeconds((long)(decimal)kvp.Value):R})"
                    : kvp.Value.ToString()!,
                _payloadKeyDescription.GetValueOrDefault(kvp.Key)))
            .ToArray();
        var result = new JwtInfo(headerValues, payloadValues);
        return result;
    }
}

[JsonSerializable(typeof(JWT.Builder.JwtHeader))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSourceGenerationOptions(WriteIndented = true, Converters = [typeof(DictionaryStringObjectJsonConverterCustomWrite)])]
internal partial class JwtJsonSerializerContext : JsonSerializerContext;

internal class AotSystemTextSerializer : IJsonSerializer
{
    /// <inheritdoc />
    public string Serialize(object obj)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public object Deserialize(Type type, string json)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        if (String.IsNullOrEmpty(json))
            throw new ArgumentException(nameof(json));

        return JsonSerializer.Deserialize(json, type, JwtJsonSerializerContext.Default)!;
    }
}