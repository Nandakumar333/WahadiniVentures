using Microsoft.EntityFrameworkCore;
using WahadiniCryptoQuest.Core.Entities;
using WahadiniCryptoQuest.Core.Enums;
using WahadiniCryptoQuest.DAL.Context;

namespace WahadiniCryptoQuest.DAL.Seeders;

/// <summary>
/// Seeds sample courses and lessons for testing
/// </summary>
public class CourseDataSeeder
{
    private readonly ApplicationDbContext _context;

    public CourseDataSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Skip if courses already exist
        if (await _context.Courses.AnyAsync())
            return;

        // Get admin user as course creator
        var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@wahadinicryptoquest.com");
        if (adminUser == null)
            return;

        // Get categories
        var categories = await _context.Categories.ToListAsync();
        if (!categories.Any())
            return;

        var airdropsCategory = categories.FirstOrDefault(c => c.Name == "Airdrops");
        var gameFiCategory = categories.FirstOrDefault(c => c.Name == "GameFi");
        var defiCategory = categories.FirstOrDefault(c => c.Name == "DeFi");
        var nftCategory = categories.FirstOrDefault(c => c.Name == "NFT Strategies");

        var courses = new List<Course>();

        // Course 1: Introduction to Crypto Airdrops
        if (airdropsCategory != null)
        {
            var course1 = new Course
            {
                Id = Guid.NewGuid(),
                Title = "Introduction to Crypto Airdrops",
                Description = "Learn how to find, participate in, and maximize your earnings from cryptocurrency airdrops. This comprehensive course covers everything from basic concepts to advanced strategies.",
                CategoryId = airdropsCategory.Id,
                DifficultyLevel = DifficultyLevel.Beginner,
                IsPremium = false,
                ThumbnailUrl = "https://images.unsplash.com/photo-1621761191319-c6fb62004040?w=800&q=80",
                RewardPoints = 100,
                EstimatedDuration = 120,
                ViewCount = 1250,
                IsPublished = true,
                CreatedByUserId = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            course1.Lessons = new List<Lesson>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CourseId = course1.Id,
                    Title = "What are Crypto Airdrops?",
                    Description = "Understanding the basics of cryptocurrency airdrops and why projects distribute free tokens",
                    YouTubeVideoId = "dQw4w9WgXcQ",
                    Duration = 15,
                    VideoDuration = 900,
                    OrderIndex = 1,
                    IsPremium = false,
                    RewardPoints = 20,
                    ContentMarkdown = "# What are Crypto Airdrops?\n\nAirdrops are a distribution method where cryptocurrency projects give away free tokens to community members...",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Tasks = new List<LearningTask>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Title = "Airdrop Basics Quiz",
                            Description = "Test your knowledge about crypto airdrops",
                            TaskType = TaskType.Quiz,
                            TaskData = "{\"questions\":[{\"id\":1,\"text\":\"What is an airdrop?\",\"options\":[\"Free tokens\",\"A scam\",\"A mining method\"],\"correctAnswer\":0}]}",
                            RewardPoints = 10,
                            OrderIndex = 1,
                            IsRequired = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CourseId = course1.Id,
                    Title = "Finding Airdrop Opportunities",
                    Description = "Learn where to find legitimate airdrop opportunities and avoid scams",
                    YouTubeVideoId = "dQw4w9WgXcQ",
                    Duration = 20,
                    VideoDuration = 1200,
                    OrderIndex = 2,
                    IsPremium = false,
                    RewardPoints = 25,
                    ContentMarkdown = "# Finding Airdrop Opportunities\n\nDiscover the best platforms and communities to find legitimate airdrops...",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CourseId = course1.Id,
                    Title = "Participating in Airdrops",
                    Description = "Step-by-step guide to participating in airdrops and claiming your tokens",
                    YouTubeVideoId = "dQw4w9WgXcQ",
                    Duration = 25,
                    VideoDuration = 1500,
                    OrderIndex = 3,
                    IsPremium = false,
                    RewardPoints = 30,
                    ContentMarkdown = "# Participating in Airdrops\n\nFollow these steps to successfully participate in cryptocurrency airdrops...",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            courses.Add(course1);
        }

        // Course 2: GameFi Fundamentals (Premium)
        if (gameFiCategory != null)
        {
            var course2 = new Course
            {
                Id = Guid.NewGuid(),
                Title = "GameFi Fundamentals: Play-to-Earn Gaming",
                Description = "Discover the world of GameFi and learn how to earn cryptocurrency by playing blockchain games. Master the strategies used by top players.",
                CategoryId = gameFiCategory.Id,
                DifficultyLevel = DifficultyLevel.Intermediate,
                IsPremium = true,
                ThumbnailUrl = "https://images.unsplash.com/photo-1538481199705-c710c4e965fc?w=800&q=80",
                RewardPoints = 200,
                EstimatedDuration = 180,
                ViewCount = 850,
                IsPublished = true,
                CreatedByUserId = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            course2.Lessons = new List<Lesson>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CourseId = course2.Id,
                    Title = "Introduction to GameFi",
                    Description = "Understanding the Play-to-Earn gaming revolution",
                    YouTubeVideoId = "dQw4w9WgXcQ",
                    Duration = 20,
                    VideoDuration = 1200,
                    OrderIndex = 1,
                    IsPremium = true,
                    RewardPoints = 40,
                    ContentMarkdown = "# Introduction to GameFi\n\nGameFi combines gaming with DeFi mechanics...",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CourseId = course2.Id,
                    Title = "Top GameFi Projects",
                    Description = "Exploring the most popular and profitable blockchain games",
                    YouTubeVideoId = "dQw4w9WgXcQ",
                    Duration = 30,
                    VideoDuration = 1800,
                    OrderIndex = 2,
                    IsPremium = true,
                    RewardPoints = 50,
                    ContentMarkdown = "# Top GameFi Projects\n\nAnalyze the leading play-to-earn games...",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CourseId = course2.Id,
                    Title = "GameFi Earning Strategies",
                    Description = "Advanced strategies to maximize your earnings in blockchain games",
                    YouTubeVideoId = "dQw4w9WgXcQ",
                    Duration = 35,
                    VideoDuration = 2100,
                    OrderIndex = 3,
                    IsPremium = true,
                    RewardPoints = 60,
                    ContentMarkdown = "# GameFi Earning Strategies\n\nLearn pro tips to increase your gaming income...",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            courses.Add(course2);
        }

        // Course 3: DeFi Basics
        if (defiCategory != null)
        {
            var course3 = new Course
            {
                Id = Guid.NewGuid(),
                Title = "DeFi Basics: Decentralized Finance Explained",
                Description = "Master the fundamentals of DeFi, including lending, borrowing, yield farming, and liquidity provision on decentralized platforms.",
                CategoryId = defiCategory.Id,
                DifficultyLevel = DifficultyLevel.Beginner,
                IsPremium = false,
                ThumbnailUrl = "https://images.unsplash.com/photo-1639762681485-074b7f938ba0?w=800&q=80",
                RewardPoints = 150,
                EstimatedDuration = 150,
                ViewCount = 2100,
                IsPublished = true,
                CreatedByUserId = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            course3.Lessons = new List<Lesson>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CourseId = course3.Id,
                    Title = "What is DeFi?",
                    Description = "Introduction to Decentralized Finance and its advantages",
                    YouTubeVideoId = "dQw4w9WgXcQ",
                    Duration = 18,
                    VideoDuration = 1080,
                    OrderIndex = 1,
                    IsPremium = false,
                    RewardPoints = 30,
                    ContentMarkdown = "# What is DeFi?\n\nDecentralized Finance (DeFi) revolutionizes traditional banking...",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CourseId = course3.Id,
                    Title = "DeFi Protocols and Platforms",
                    Description = "Explore popular DeFi protocols like Uniswap, Aave, and Compound",
                    YouTubeVideoId = "dQw4w9WgXcQ",
                    Duration = 25,
                    VideoDuration = 1500,
                    OrderIndex = 2,
                    IsPremium = false,
                    RewardPoints = 40,
                    ContentMarkdown = "# DeFi Protocols and Platforms\n\nLearn about the leading DeFi platforms...",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CourseId = course3.Id,
                    Title = "Yield Farming Basics",
                    Description = "Understanding yield farming and how to earn passive income",
                    YouTubeVideoId = "dQw4w9WgXcQ",
                    Duration = 28,
                    VideoDuration = 1680,
                    OrderIndex = 3,
                    IsPremium = false,
                    RewardPoints = 45,
                    ContentMarkdown = "# Yield Farming Basics\n\nDiscover strategies for yield farming...",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            courses.Add(course3);
        }

        // Course 4: NFT Investment Strategies (Premium)
        if (nftCategory != null)
        {
            var course4 = new Course
            {
                Id = Guid.NewGuid(),
                Title = "NFT Investment Strategies",
                Description = "Learn advanced NFT investment techniques, market analysis, and how to identify valuable digital collectibles before they explode in value.",
                CategoryId = nftCategory.Id,
                DifficultyLevel = DifficultyLevel.Advanced,
                IsPremium = true,
                ThumbnailUrl = "https://images.unsplash.com/photo-1620641788421-7a1c342ea42e?w=800&q=80",
                RewardPoints = 250,
                EstimatedDuration = 200,
                ViewCount = 650,
                IsPublished = true,
                CreatedByUserId = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            course4.Lessons = new List<Lesson>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CourseId = course4.Id,
                    Title = "NFT Market Analysis",
                    Description = "Analyzing NFT market trends and identifying opportunities",
                    YouTubeVideoId = "dQw4w9WgXcQ",
                    Duration = 30,
                    VideoDuration = 1800,
                    OrderIndex = 1,
                    IsPremium = true,
                    RewardPoints = 50,
                    ContentMarkdown = "# NFT Market Analysis\n\nMaster the art of NFT market research...",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CourseId = course4.Id,
                    Title = "Valuing NFT Collections",
                    Description = "Techniques for determining the value of NFT projects",
                    YouTubeVideoId = "dQw4w9WgXcQ",
                    Duration = 35,
                    VideoDuration = 2100,
                    OrderIndex = 2,
                    IsPremium = true,
                    RewardPoints = 60,
                    ContentMarkdown = "# Valuing NFT Collections\n\nLearn valuation frameworks for NFTs...",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CourseId = course4.Id,
                    Title = "Advanced NFT Trading",
                    Description = "Pro strategies for buying, selling, and flipping NFTs",
                    YouTubeVideoId = "dQw4w9WgXcQ",
                    Duration = 40,
                    VideoDuration = 2400,
                    OrderIndex = 3,
                    IsPremium = true,
                    RewardPoints = 70,
                    ContentMarkdown = "# Advanced NFT Trading\n\nPro tips from successful NFT traders...",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            courses.Add(course4);
        }

        // Add all courses to database
        _context.Courses.AddRange(courses);
        await _context.SaveChangesAsync();
    }
}
