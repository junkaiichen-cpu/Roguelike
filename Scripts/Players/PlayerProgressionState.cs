using System;

public readonly record struct PlayerProgressionResult(uint ExperienceAdded, uint LevelsGained)
{
    public bool HasLeveledUp => LevelsGained > 0;
}

public sealed class PlayerProgressionState
{
    private readonly uint _experienceRequirementIncreasePerLevel;

    public uint CurrentExperience { get; private set; }

    public uint CurrentLevel { get; private set; }

    public uint ExperienceRequiredForNextLevel { get; private set; }

    public PlayerProgressionState(
        uint initialLevel,
        uint initialExperienceRequired,
        uint experienceRequirementIncreasePerLevel)
    {
        if (initialLevel == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialLevel), "Initial level must be greater than zero.");
        }

        if (initialExperienceRequired == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(initialExperienceRequired),
                "Experience required for the next level must be greater than zero.");
        }

        if (experienceRequirementIncreasePerLevel == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(experienceRequirementIncreasePerLevel),
                "Experience requirement increase must be greater than zero.");
        }

        CurrentLevel = initialLevel;
        ExperienceRequiredForNextLevel = initialExperienceRequired;
        _experienceRequirementIncreasePerLevel = experienceRequirementIncreasePerLevel;
    }

    public PlayerProgressionResult AddExperience(long experience)
    {
        if (experience < 0 || experience > uint.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(experience), "Experience must be between zero and UInt32.MaxValue.");
        }

        if (experience == 0)
        {
            return new PlayerProgressionResult(0, 0);
        }

        ulong accumulatedExperience = CurrentExperience + (ulong)experience;
        uint levelsGained = 0;

        while (accumulatedExperience >= ExperienceRequiredForNextLevel)
        {
            accumulatedExperience -= ExperienceRequiredForNextLevel;
            CurrentLevel = checked(CurrentLevel + 1);
            ExperienceRequiredForNextLevel = IncreaseThreshold(ExperienceRequiredForNextLevel);
            levelsGained++;
        }

        CurrentExperience = (uint)accumulatedExperience;
        return new PlayerProgressionResult((uint)experience, levelsGained);
    }

    private uint IncreaseThreshold(uint currentThreshold)
    {
        if (uint.MaxValue - currentThreshold < _experienceRequirementIncreasePerLevel)
        {
            return uint.MaxValue;
        }

        return currentThreshold + _experienceRequirementIncreasePerLevel;
    }
}
