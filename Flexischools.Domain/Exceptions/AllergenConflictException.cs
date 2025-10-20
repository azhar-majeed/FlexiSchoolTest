namespace Flexischools.Domain.Exceptions;

public class AllergenConflictException : Exception
{
    public string StudentName { get; }
    public string MenuItemName { get; }
    public string ConflictingAllergens { get; }
    
    public AllergenConflictException(string studentName, string menuItemName, string conflictingAllergens)
        : base($"Allergen conflict for student '{studentName}' with menu item '{menuItemName}'. Conflicting allergens: {conflictingAllergens}")
    {
        StudentName = studentName;
        MenuItemName = menuItemName;
        ConflictingAllergens = conflictingAllergens;
    }
}
