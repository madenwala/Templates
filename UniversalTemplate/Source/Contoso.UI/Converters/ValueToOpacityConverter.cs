namespace Contoso.UI.Converters
{
    public sealed class ValueToOpacityConverter : BooleanToObjectConverter<double>
    {
        public ValueToOpacityConverter()
        {
            this.TrueValue = 1;
            this.FalseValue = .3;
        }
    }
}
