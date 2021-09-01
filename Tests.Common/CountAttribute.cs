namespace Tests.Common
{
    using System.Reflection;
    using AutoFixture;
    using AutoFixture.Xunit2;

    public class CountAttribute : CustomizeAttribute
    {
        private readonly int count;

        public CountAttribute(int count)
        {
            this.count = count;
        }

        public override ICustomization GetCustomization(ParameterInfo parameter) => new CountCustomization(this.count);

        class CountCustomization : ICustomization
        {
            private readonly int count;

            public CountCustomization(int count)
            {
                this.count = count;
            }

            public void Customize(IFixture fixture)
            {
                fixture.RepeatCount = this.count;
            }
        }
    }
}
