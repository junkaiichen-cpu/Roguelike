public interface ITemporaryUpgradeReceiver
{
    bool TryApplyTemporaryUpgrade(TemporaryUpgradeDefinition upgrade);
}
