namespace Server.Enumes
{
    //exemple of use
    //using Server.Enumes;
    //var startup = TypeCompany.Startup;
    //Console.WriteLine($"{startup.Type} => {startup.NumberConsultant} consultants, {startup.Treasury} treasury");

    public enum CompanyType { Startup, SME, Corporation, Enterprise }

    public sealed class TypeCompany
    {
        public CompanyType Type { get; }
        public int NumberConsultant { get; }
        public int Treasury { get; }

        private TypeCompany(CompanyType type, int numberConsultant, int treasury)
        {
            Type = type;
            NumberConsultant = numberConsultant;
            Treasury = treasury;
        }

        //#todo adapt value of equilibrage
        public static readonly TypeCompany Startup = new(CompanyType.Startup, 3, 10000);
        public static readonly TypeCompany SME = new(CompanyType.SME, 10, 50000);
        public static readonly TypeCompany Corporation = new(CompanyType.Corporation, 50, 250000);
        public static readonly TypeCompany Enterprise = new(CompanyType.Enterprise, 200, 1000000);

        public static TypeCompany From(CompanyType type) => type switch
        {
            CompanyType.Startup => Startup,
            CompanyType.SME => SME,
            CompanyType.Corporation => Corporation,
            CompanyType.Enterprise => Enterprise,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
