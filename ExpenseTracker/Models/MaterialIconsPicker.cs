namespace ExpenseTracker.Models;

public class MaterialIconsPicker
{
    public static List<IconPickerGroup> Groups { get; } =
    [
        new("Food & Drink",
        [
            new("\ue2e7", "Grocery"),
            new("\ue56c", "Restaurant"),
            new("\ue53a", "Fast Food"),
            new("\uea61", "Coffee"),
            new("\ue540", "Pizza"),
            new("\uf7b8", "Bakery"),
            new("\ue544", "Liquor"),
            new("\ue57a", "Takeaway"),
        ]),

        new("Transport",
        [
            new("\ue531", "Car"),
            new("\ue558", "Gas Station"),
            new("\ue570", "Train"),
            new("\ue533", "Bus"),
            new("\ue559", "Flight"),
            new("\ue567", "Taxi"),
            new("\ueb29", "Bike"),
            new("\ue57c", "Parking"),
        ]),

        new("Home & Bills",
        [
            new("\ue88a", "Home"),
            new("\ue8f0", "Water"),
            new("\ue63f", "Electric"),
            new("\ue87b", "WiFi"),
            new("\ue88e", "Lightbulb"),
            new("\ue8d5", "Security"),
            new("\uea40", "Repair"),
            new("\ue8b8", "Settings"),
        ]),

        new("Health & Fitness",
        [
            new("\ue548", "Hospital"),
            new("\ue8ff", "Pharmacy"),
            new("\ue87d", "Fitness"),
            new("\uf1bb", "Yoga"),
            new("\uea78", "Running"),
            new("\ue54e", "Dentist"),
            new("\ue8cd", "Vision"),
            new("\uf44b", "Mental Health"),
        ]),

        new("Shopping",
        [
            new("\ue8cc", "Shopping"),
            new("\uf290", "Shopping Bag"),
            new("\ue8d8", "Clothing"),
            new("\uef5b", "Footwear"),
            new("\ue3ab", "Jewellery"),
            new("\ue326", "Electronics"),
            new("\ue59c", "Furniture"),
            new("\uea3e", "Tools"),
        ]),

        new("Entertainment",
        [
            new("\ue325", "Streaming"),
            new("\ue02c", "Music"),
            new("\ue54c", "Movies"),
            new("\uf135", "Gaming"),
            new("\ue8f9", "Travel"),
            new("\ue1bc", "Books"),
            new("\ue80c", "Sports"),
            new("\ue903", "Events"),
        ]),

        new("Personal",
        [
            new("\ue7ef", "Person"),
            new("\ue7fd", "Family"),
            new("\ue873", "Pets"),
            new("\ue3ae", "Beauty"),
            new("\uea76", "Spa"),
            new("\ue865", "Education"),
            new("\ue869", "Star"),
            new("\ue87e", "Gift"),
        ]),

        new("Finance",
        [
            new("\ue227", "Bank"),
            new("\ue263", "Credit Card"),
            new("\ue8dc", "Savings"),
            new("\ue851", "Investment"),
            new("\ue57d", "Insurance"),
            new("\ue908", "Tax"),
            new("\ue8b1", "Subscription"),
            new("\ue8a1", "Wallet"),
        ]),

        new("Other",
        [
            new("\ue5d3", "Other"),
            new("\ue614", "Work"),
            new("\ue87c", "Charity"),
            new("\ue545", "Kids"),
            new("\ue53b", "Baby"),
            new("\ue1d5", "Laptop"),
            new("\ue30a", "Camera"),
            new("\ue87a", "Headphones"),
        ]),
    ];

    // Flat list for search across all groups
    public static List<IconPickerItem> All =>
        [.. Groups.SelectMany(g => g.Icons)];
}

public record IconPickerGroup(string Name, List<IconPickerItem> Icons);
public record IconPickerItem(string Glyph, string Label);
