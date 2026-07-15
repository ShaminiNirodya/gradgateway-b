namespace GradGateway.Data.Seeding;

public static class AcademicCatalogSeed
{
    /// <summary>Initial Sri Lankan state university → IT/CS degree offerings.</summary>
    public static IReadOnlyDictionary<string, string[]> UniversityDegrees { get; } =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["University of Sri Jayewardenepura"] =
            [
                "Arts - Information Technology",
                "Business Information Systems (Honours) (BIS)",
                "Computer Science",
                "Physical Science - ICT",
                "Information Systems",
                "Software Engineering",
                "Bachelor of Science Honours in Business Information Systems",
                "Bachelor of Science Honours in Information Systems",
                "Biosystems Technology (BST)",
                "Engineering Technology (ET)",
            ],
            ["University of Kelaniya"] =
            [
                "Accounting Information Systems",
                "Electronics and Computer Science",
                "Software Engineering",
                "Management and Information Technology",
            ],
            ["University of Peradeniya"] =
            [
                "Engineering",
                "Computer Science",
                "Electronic and Intelligent Systems Engineering",
            ],
            ["University of Jaffna"] = ["Computer Science", "Biosystems Technology (BST)"],
            ["University of Ruhuna"] = ["Computer Science", "Biosystems Technology (BST)"],
            ["University of Moratuwa"] =
            [
                "Transport Management & Logistics Engineering (TMLE)",
                "Information Technology (IT)",
                "Information Technology & Management",
                "Artificial Intelligence",
                "Electronics and Telecommunication Engineering",
            ],
            ["South Eastern University of Sri Lanka"] = ["Management and Information Technology (SEUSL)"],
            ["University of Colombo"] = ["Physical Science", "Biosystems Technology (BST)"],
            ["University of Colombo School of Computing"] = ["Computer Science", "Information Systems"],
            ["Trincomalee Campus, Eastern University, Sri Lanka"] =
            [
                "Computer Science",
                "Bachelor of Science Honours in Information Systems",
            ],
            ["Rajarata University of Sri Lanka"] =
            [
                "Bachelor of Science Honours in Information Systems",
                "Information and Communication Technology (ICT)",
                "Biosystems Technology (BST)",
            ],
            ["Eastern University, Sri Lanka"] = ["Computer Science", "Biosystems Technology (BST)"],
            ["Sabaragamuwa University of Sri Lanka"] =
            [
                "Data Science",
                "Computer Science",
                "Information Systems",
                "Software Engineering",
                "Biosystems Technology (BST)",
            ],
            ["Uva Wellassa University of Sri Lanka"] =
            [
                "Science and Technology",
                "Computer Science & Technology",
                "Industrial Information Technology",
                "Biosystems Technology (BST)",
            ],
            ["University of Vavuniya, Sri Lanka"] = ["Information and Communication Technology (ICT)"],
            ["Wayamba University of Sri Lanka"] = ["Biosystems Technology (BST)"],
            ["Sripalee Campus, University of Colombo"] = ["Computer Science"],
            ["Swamy Vipulananda Institute of Aesthetic Studies, Eastern University, Sri Lanka"] =
                ["Biosystems Technology (BST)"],
            ["University of the Visual & Performing Arts"] = [],
            ["Gampaha Wickramarachchi University of Indigenous Medicine, Sri Lanka"] = [],
            ["The Open University of Sri Lanka"] = [],
        };
}
