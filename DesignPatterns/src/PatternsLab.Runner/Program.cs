using System.Diagnostics;
using PatternsLab.Problems.Builder;
using PatternsLab.Problems.Prototype;
using PatternsLab.Problems.Singleton;

// ========================================
// SINGLETON
// ========================================

Console.WriteLine("=== SINGLETON ===\n");

var db = new DatabaseService();
var ui = new UiService();

Console.WriteLine();

db.Config.Theme = "Dark";

Console.WriteLine("Admin changed theme to Dark.");

ui.Render();

Console.WriteLine(
    $"\nSame config object? " +
    $"{ReferenceEquals(db.Config, ui.Config)}");

Console.WriteLine(
    $"Times config was loaded from disk: {AppConfig.LoadCount}");


// ========================================
// PROTOTYPE
// ========================================

Console.WriteLine("\n=== PROTOTYPE ===\n");

// 1. Create the registry.
var registry = new EnemyRegistry();

// 2. Create and register the prototypes.
// Each constructor loads its model only once.

registry.Register("Orc", new Orc());
registry.Register("Elf", new Elf());

// 3. Create an army by cloning the Orc prototype.

var sw = Stopwatch.StartNew();

var army = new List<Enemy>();

for (int i = 1; i <= 5; i++)
{
    Enemy orc = registry.CreateEnemy("Orc");

    orc.Name = $"Orc-{i}";

    army.Add(orc);
}

sw.Stop();

Console.WriteLine(
    $"Created {army.Count} orcs in {sw.ElapsedMilliseconds} ms");

// 4. Display the created enemies.

Console.WriteLine("\nArmy:");

foreach (Enemy enemy in army)
{
    Console.WriteLine(
        $"{enemy.Name} | Model: {enemy.ModelId} | " +
        $"Weapon: {enemy.Weapon.Name} | " +
        $"Damage: {enemy.Weapon.Damage}");
}

// 5. Test that the clone has the same ModelId as the prototype.

Enemy original = registry.CreateEnemy("Orc");

original.Name = "Boss Orc";

Enemy copy = EnemyCopyHelper.CopyEnemy(original);

Console.WriteLine("\nClone tests:");

Console.WriteLine(
    $"Original model id: {original.ModelId}");

Console.WriteLine(
    $"Copy model id:     {copy.ModelId}");

Console.WriteLine(
    $"Same model id? " +
    $"{original.ModelId == copy.ModelId}");

Console.WriteLine(
    $"Same Enemy object? " +
    $"{ReferenceEquals(original, copy)}");

Console.WriteLine(
    $"Same Weapon object? " +
    $"{ReferenceEquals(original.Weapon, copy.Weapon)}");

Console.WriteLine(
    $"Same Abilities list? " +
    $"{ReferenceEquals(original.Abilities, copy.Abilities)}");

// 6. Test deep copying.

copy.Weapon.Damage = 999;

Console.WriteLine("\nAfter changing the copy's weapon damage:");

Console.WriteLine($"Copy damage:     {copy.Weapon.Damage}");

Console.WriteLine($"Original damage: {original.Weapon.Damage}");

copy.Abilities.Add("Fire Breath");

Console.WriteLine("\nAfter adding an ability to the copy:");

Console.WriteLine(
    $"Copy abilities:     {string.Join(", ", copy.Abilities)}");

Console.WriteLine(
    $"Original abilities: {string.Join(", ", original.Abilities)}");

// 7. Test that the registry can create different enemy types.

Enemy elf = registry.CreateEnemy("Elf");

Console.WriteLine(
    $"\nCreated from registry: {elf.Name}");

// ========================================
// BUILDER
// ========================================

Console.WriteLine("\n=== BUILDER ===\n");

Console.WriteLine(RegistrationCallSites.CreateLiveStudentUgly());

Console.WriteLine(RegistrationCallSites.CreateVideosOnlyUgly());