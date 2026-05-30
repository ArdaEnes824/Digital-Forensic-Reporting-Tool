using DFIR.CaseManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace DFIR.CaseManagement.Data;

/// <summary>Seeds realistic-looking but entirely fictional forensic case data for demonstration.</summary>
public static class SyntheticDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SyntheticDataSeeder");

        if (await context.Cases.AnyAsync())
        {
            logger.LogInformation("Synthetic seed skipped: cases already present.");
            return;
        }

        // ── Cases ──────────────────────────────────────────────────────────
        var cases = new List<Case>
        {
            new()
            {
                CaseNumber = "DFIR-2025-001",
                Title = "Kurumsal Ağ Sızma Olayı",
                Description = "Finans departmanı sunucusunda yetkisiz erişim tespit edildi. " +
                              "Saldırgan lateral movement yaparak domain controller'a ulaştı.",
                Status = CaseStatus.InProgress,
                Priority = CasePriority.Critical,
                CurrentStage = WorkflowStage.Analyze,
                AssignedTo = "Kemal Aydın",
                CreatedDate = DateTime.UtcNow.AddDays(-45)
            },
            new()
            {
                CaseNumber = "DFIR-2025-002",
                Title = "Fidye Yazılımı Saldırısı - Üretim Tesisi",
                Description = "LockBit 3.0 varyantı üretim hattı SCADA sistemlerini şifreledi. " +
                              "Toplam 147 endpoint etkilendi, yedekleme sunucuları da hedef alındı.",
                Status = CaseStatus.InProgress,
                Priority = CasePriority.Critical,
                CurrentStage = WorkflowStage.Preserve,
                AssignedTo = "Selin Çelik",
                CreatedDate = DateTime.UtcNow.AddDays(-30)
            },
            new()
            {
                CaseNumber = "DFIR-2025-003",
                Title = "İçeriden Veri Sızdırma - İK Veritabanı",
                Description = "Ayrılan çalışanın 90.000 çalışanın kişisel verisini USB belleğe " +
                              "kopyaladığı DLP alarmlarıyla tespit edildi.",
                Status = CaseStatus.Closed,
                Priority = CasePriority.High,
                CurrentStage = WorkflowStage.Report,
                AssignedTo = "Murat Yıldız",
                CreatedDate = DateTime.UtcNow.AddDays(-90)
            },
            new()
            {
                CaseNumber = "DFIR-2025-004",
                Title = "Phishing Kampanyası - Yönetici Hesapları",
                Description = "CEO ve CFO dahil 12 yönetici hesabı hedef alındı. " +
                              "BEC saldırısıyla sahte IBAN değişikliği talebi gönderildi.",
                Status = CaseStatus.Open,
                Priority = CasePriority.High,
                CurrentStage = WorkflowStage.Collect,
                AssignedTo = "Ayşe Kara",
                CreatedDate = DateTime.UtcNow.AddDays(-7)
            },
            new()
            {
                CaseNumber = "DFIR-2025-005",
                Title = "Web Uygulama Güvenlik İhlali",
                Description = "E-ticaret platformunda SQL injection açığı istismar edildi. " +
                              "350.000 müşteri kredi kartı verisi çalındı, Dark Web'de satışa çıkarıldı.",
                Status = CaseStatus.InProgress,
                Priority = CasePriority.Critical,
                CurrentStage = WorkflowStage.Interpret,
                AssignedTo = "Burak Demir",
                CreatedDate = DateTime.UtcNow.AddDays(-20)
            },
            new()
            {
                CaseNumber = "DFIR-2025-006",
                Title = "Mobil Cihaz Adli Analizi - Suç Soruşturması",
                Description = "Organize suç soruşturması kapsamında el konulan 3 akıllı telefonun " +
                              "şifreli mesajlaşma uygulamaları ve silinen verileri inceleniyor.",
                Status = CaseStatus.InProgress,
                Priority = CasePriority.Medium,
                CurrentStage = WorkflowStage.Acquire,
                AssignedTo = "Zeynep Arslan",
                CreatedDate = DateTime.UtcNow.AddDays(-15)
            },
            new()
            {
                CaseNumber = "DFIR-2025-007",
                Title = "Kripto Madencilik Botnet Tespiti",
                Description = "250 sunucuya Monero madencisi yerleştirilmiş. " +
                              "Aylık 180.000 TL elektrik maliyeti artışı fark edilmesiyle keşfedildi.",
                Status = CaseStatus.Closed,
                Priority = CasePriority.Medium,
                CurrentStage = WorkflowStage.Report,
                AssignedTo = "Emre Şahin",
                CreatedDate = DateTime.UtcNow.AddDays(-60)
            },
            new()
            {
                CaseNumber = "DFIR-2025-008",
                Title = "Tedarik Zinciri Saldırısı - Yazılım Güncellemesi",
                Description = "Üçüncü taraf muhasebe yazılımının güncelleme mekanizması ele geçirildi. " +
                              "Truva atı içeren update paketi 1.200 kurumsal müşteriye dağıtıldı.",
                Status = CaseStatus.Open,
                Priority = CasePriority.Critical,
                CurrentStage = WorkflowStage.Identify,
                AssignedTo = "Fatma Özkan",
                CreatedDate = DateTime.UtcNow.AddDays(-3)
            }
        };

        await context.Cases.AddRangeAsync(cases);
        await context.SaveChangesAsync();

        // ── Evidence ────────────────────────────────────────────────────────
        var evidence = new List<Evidence>();

        // Case 1 - Ağ sızma
        var e1 = new Evidence { CaseId = cases[0].Id, EvidenceCode = "EVD-2025-001-01", DeviceType = "Sunucu", Manufacturer = "Dell", Model = "PowerEdge R750", SerialNumber = "DLPS-8X9K2-1A", CreatedDate = DateTime.UtcNow.AddDays(-44) };
        e1.GenerateHashesFromText("DLPS-8X9K2-1A");
        evidence.Add(e1);

        var e2 = new Evidence { CaseId = cases[0].Id, EvidenceCode = "EVD-2025-001-02", DeviceType = "Ağ Cihazı", Manufacturer = "Cisco", Model = "Catalyst 9300", SerialNumber = "FCW2317G0AB", CreatedDate = DateTime.UtcNow.AddDays(-44) };
        e2.GenerateHashesFromText("FCW2317G0AB");
        evidence.Add(e2);

        var e3 = new Evidence { CaseId = cases[0].Id, EvidenceCode = "EVD-2025-001-03", DeviceType = "Dizüstü Bilgisayar", Manufacturer = "Lenovo", Model = "ThinkPad X1 Carbon", SerialNumber = "LNV-MP2QR-77", CreatedDate = DateTime.UtcNow.AddDays(-43) };
        e3.GenerateHashesFromText("LNV-MP2QR-77");
        evidence.Add(e3);

        // Case 2 - Fidye yazılımı
        var e4 = new Evidence { CaseId = cases[1].Id, EvidenceCode = "EVD-2025-002-01", DeviceType = "Uç Nokta", Manufacturer = "HP", Model = "EliteBook 840 G9", SerialNumber = "HP-5CG2441PBX", CreatedDate = DateTime.UtcNow.AddDays(-29) };
        e4.GenerateHashesFromText("HP-5CG2441PBX");
        evidence.Add(e4);

        var e5 = new Evidence { CaseId = cases[1].Id, EvidenceCode = "EVD-2025-002-02", DeviceType = "SCADA HMI", Manufacturer = "Siemens", Model = "SIMATIC IPC477E", SerialNumber = "SIE-2X7K9-SCADA", CreatedDate = DateTime.UtcNow.AddDays(-29) };
        e5.GenerateHashesFromText("SIE-2X7K9-SCADA");
        evidence.Add(e5);

        // Case 3 - Veri sızdırma
        var e6 = new Evidence { CaseId = cases[2].Id, EvidenceCode = "EVD-2025-003-01", DeviceType = "USB Bellek", Manufacturer = "SanDisk", Model = "Ultra 128GB", SerialNumber = "SDCZ48-128G-A46", CreatedDate = DateTime.UtcNow.AddDays(-88) };
        e6.GenerateHashesFromText("SDCZ48-128G-A46");
        evidence.Add(e6);

        var e7 = new Evidence { CaseId = cases[2].Id, EvidenceCode = "EVD-2025-003-02", DeviceType = "Masaüstü Bilgisayar", Manufacturer = "Dell", Model = "OptiPlex 7090", SerialNumber = "DL-7090-3XF9P", CreatedDate = DateTime.UtcNow.AddDays(-87) };
        e7.GenerateHashesFromText("DL-7090-3XF9P");
        evidence.Add(e7);

        // Case 4 - Phishing
        var e8 = new Evidence { CaseId = cases[3].Id, EvidenceCode = "EVD-2025-004-01", DeviceType = "Akıllı Telefon", Manufacturer = "Apple", Model = "iPhone 15 Pro", SerialNumber = "APL-F9K2M-PRO15", CreatedDate = DateTime.UtcNow.AddDays(-6) };
        e8.GenerateHashesFromText("APL-F9K2M-PRO15");
        evidence.Add(e8);

        // Case 5 - Web saldırısı
        var e9 = new Evidence { CaseId = cases[4].Id, EvidenceCode = "EVD-2025-005-01", DeviceType = "Web Sunucu", Manufacturer = "Supermicro", Model = "SuperServer 2029P", SerialNumber = "SMC-2029P-0WX29", CreatedDate = DateTime.UtcNow.AddDays(-19) };
        e9.GenerateHashesFromText("SMC-2029P-0WX29");
        evidence.Add(e9);

        var e10 = new Evidence { CaseId = cases[4].Id, EvidenceCode = "EVD-2025-005-02", DeviceType = "Veritabanı Sunucu", Manufacturer = "IBM", Model = "Power System S922", SerialNumber = "IBM-S922-9009-22A", CreatedDate = DateTime.UtcNow.AddDays(-18) };
        e10.GenerateHashesFromText("IBM-S922-9009-22A");
        evidence.Add(e10);

        // Case 6 - Mobil
        var e11 = new Evidence { CaseId = cases[5].Id, EvidenceCode = "EVD-2025-006-01", DeviceType = "Akıllı Telefon", Manufacturer = "Samsung", Model = "Galaxy S23 Ultra", SerialNumber = "SAM-S23U-R9T7K", CreatedDate = DateTime.UtcNow.AddDays(-14) };
        e11.GenerateHashesFromText("SAM-S23U-R9T7K");
        evidence.Add(e11);

        var e12 = new Evidence { CaseId = cases[5].Id, EvidenceCode = "EVD-2025-006-02", DeviceType = "Akıllı Telefon", Manufacturer = "Apple", Model = "iPhone 14", SerialNumber = "APL-14-KM3P9-BLK", CreatedDate = DateTime.UtcNow.AddDays(-14) };
        e12.GenerateHashesFromText("APL-14-KM3P9-BLK");
        evidence.Add(e12);

        // Case 7 - Botnet
        var e13 = new Evidence { CaseId = cases[6].Id, EvidenceCode = "EVD-2025-007-01", DeviceType = "Sanal Makine Diski", Manufacturer = "VMware", Model = "vSphere VMDK", SerialNumber = "VMW-DS-4420-X99", CreatedDate = DateTime.UtcNow.AddDays(-58) };
        e13.GenerateHashesFromText("VMW-DS-4420-X99");
        evidence.Add(e13);

        // Case 8 - Tedarik zinciri
        var e14 = new Evidence { CaseId = cases[7].Id, EvidenceCode = "EVD-2025-008-01", DeviceType = "Yazılım Paketi", Manufacturer = "Build Server", Model = "CI/CD Artifact", SerialNumber = "BUILD-20251128-v4.2.1", CreatedDate = DateTime.UtcNow.AddDays(-2) };
        e14.GenerateHashesFromText("BUILD-20251128-v4.2.1");
        evidence.Add(e14);

        await context.Evidence.AddRangeAsync(evidence);
        await context.SaveChangesAsync();

        // ── Chain of Custody ────────────────────────────────────────────────
        var custody = new List<ChainOfCustody>
        {
            // Case 1
            new() { CaseId = cases[0].Id, FromPerson = "Güvenlik Ekibi", ToPerson = "Kemal Aydın (Baş Analist)", TransferDate = DateTime.UtcNow.AddDays(-44), Location = "Sunucu Odası - Kat 3", Description = "İlk müdahale kapsamında sunucu imajı alındı.", CreatedDate = DateTime.UtcNow.AddDays(-44) },
            new() { CaseId = cases[0].Id, FromPerson = "Kemal Aydın (Baş Analist)", ToPerson = "Adli Bilişim Laboratuvarı", TransferDate = DateTime.UtcNow.AddDays(-42), Location = "Güvenli Taşıma - Zincirli Çanta", Description = "Mühürlü delil torbası ile lab'a teslim edildi.", CreatedDate = DateTime.UtcNow.AddDays(-42) },
            new() { CaseId = cases[0].Id, FromPerson = "Adli Bilişim Laboratuvarı", ToPerson = "Savcılık - Dijital Delil Birimi", TransferDate = DateTime.UtcNow.AddDays(-20), Location = "Adli Bilişim Lab, Oda 14", Description = "Resmi zincir belgesi imzalandı.", CreatedDate = DateTime.UtcNow.AddDays(-20) },

            // Case 2
            new() { CaseId = cases[1].Id, FromPerson = "Üretim Müdürü Hasan Koç", ToPerson = "Selin Çelik (DFIR Analisti)", TransferDate = DateTime.UtcNow.AddDays(-30), Location = "Üretim Tesisi - Kontrol Odası", Description = "SCADA HMI cihazları mühürlendi, teslim tutanağı düzenlendi.", CreatedDate = DateTime.UtcNow.AddDays(-30) },
            new() { CaseId = cases[1].Id, FromPerson = "Selin Çelik (DFIR Analisti)", ToPerson = "Sigorta Eksper Ekibi", TransferDate = DateTime.UtcNow.AddDays(-25), Location = "DFIR Ofisi", Description = "Sigorta tazminat süreci için kopyalar hazırlandı.", CreatedDate = DateTime.UtcNow.AddDays(-25) },

            // Case 3
            new() { CaseId = cases[2].Id, FromPerson = "İK Müdürü Nilüfer Yılmaz", ToPerson = "Murat Yıldız (Analist)", TransferDate = DateTime.UtcNow.AddDays(-89), Location = "İK Departmanı - Depo", Description = "Çalışanın masasından toplanan USB ve bilgisayar.", CreatedDate = DateTime.UtcNow.AddDays(-89) },
            new() { CaseId = cases[2].Id, FromPerson = "Murat Yıldız (Analist)", ToPerson = "Hukuk Departmanı", TransferDate = DateTime.UtcNow.AddDays(-70), Location = "Adli Bilişim Lab", Description = "İş hukuku davası için deliller hukuk birimine iletildi.", CreatedDate = DateTime.UtcNow.AddDays(-70) },
            new() { CaseId = cases[2].Id, FromPerson = "Hukuk Departmanı", ToPerson = "Mahkeme Yazı İşleri", TransferDate = DateTime.UtcNow.AddDays(-30), Location = "Anadolu Adliyesi", Description = "Dava dosyasına eklenmek üzere mahkemeye teslim edildi.", CreatedDate = DateTime.UtcNow.AddDays(-30) },

            // Case 4
            new() { CaseId = cases[3].Id, FromPerson = "BT Güvenlik Ekibi", ToPerson = "Ayşe Kara (Analist)", TransferDate = DateTime.UtcNow.AddDays(-7), Location = "SOC Odası", Description = "Phishing e-posta örnekleri ve etkilenen cihazlar teslim alındı.", CreatedDate = DateTime.UtcNow.AddDays(-7) },

            // Case 5
            new() { CaseId = cases[4].Id, FromPerson = "Sistem Yöneticisi Ozan Tekin", ToPerson = "Burak Demir (Baş Analist)", TransferDate = DateTime.UtcNow.AddDays(-19), Location = "Veri Merkezi - Rack 7", Description = "Web ve DB sunucu diskleri forensic imaj için teslim alındı.", CreatedDate = DateTime.UtcNow.AddDays(-19) },
            new() { CaseId = cases[4].Id, FromPerson = "Burak Demir (Baş Analist)", ToPerson = "KVKK Uyum Ekibi", TransferDate = DateTime.UtcNow.AddDays(-10), Location = "Hukuk & Uyum Birimi", Description = "KVKK ihlal bildirimi hazırlığı için uyum ekibine gönderildi.", CreatedDate = DateTime.UtcNow.AddDays(-10) },

            // Case 7 (Closed)
            new() { CaseId = cases[6].Id, FromPerson = "NOC Ekibi", ToPerson = "Emre Şahin (Analist)", TransferDate = DateTime.UtcNow.AddDays(-59), Location = "Ağ Operasyon Merkezi", Description = "Botnet C2 trafiği tespit edilen sunucu görüntüsü alındı.", CreatedDate = DateTime.UtcNow.AddDays(-59) },
            new() { CaseId = cases[6].Id, FromPerson = "Emre Şahin (Analist)", ToPerson = "Kapatma Onay - CISO", TransferDate = DateTime.UtcNow.AddDays(-10), Location = "DFIR Lab", Description = "Analiz tamamlandı, CISO onayıyla vaka kapatıldı.", CreatedDate = DateTime.UtcNow.AddDays(-10) },
        };

        await context.CustodyRecords.AddRangeAsync(custody);
        await context.SaveChangesAsync();

        // ── Malware Analyses ────────────────────────────────────────────────
        var malwareItems = new List<MalwareAnalysis>();

        void AddMalware(string fileName, long fileSize, string? sha256, double riskScore, double entropy,
            RiskLevel risk, string verdict, List<string> indicators, int? caseId)
        {
            var m = new MalwareAnalysis
            {
                FileName = fileName,
                FileSize = fileSize,
                CaseId = caseId,
                CreatedDate = DateTime.UtcNow.AddDays(-new Random(fileName.Length).Next(1, 50))
            };
            m.SetAnalysisResult(sha256, riskScore, entropy, risk, verdict,
                System.Text.Json.JsonSerializer.Serialize(indicators));
            malwareItems.Add(m);
        }

        AddMalware(
            "lockbit3_dropper.exe", 524_288,
            "a3f1e2b9c847d506e1af2c3b4d5e6f708192a3b4c5d6e7f8091a2b3c4d5e6f70",
            92.4, 7.84, RiskLevel.Critical,
            "Critical (score 92.4)",
            new() { "High entropy section (possible packing/encryption)", "PE/MZ executable header detected", "Risk score exceeds High threshold", "Known ransomware dropper signature pattern" },
            cases[1].Id);

        AddMalware(
            "update_installer_v4.2.1.msi", 2_097_152,
            "b4c2d3e4f5a6b7c8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2",
            87.1, 7.62, RiskLevel.Critical,
            "Critical (score 87.1)",
            new() { "High entropy section (possible packing/encryption)", "PE/MZ executable header detected", "Risk score exceeds High threshold", "Trojanized MSI installer - supply chain artifact" },
            cases[7].Id);

        AddMalware(
            "xmrig_miner_svc.dll", 1_048_576,
            "c5d3e4f5a6b7c8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3",
            78.5, 7.41, RiskLevel.High,
            "High (score 78.5)",
            new() { "PE/MZ executable header detected", "Risk score exceeds High threshold", "Monero mining pool connection string found in strings" },
            cases[6].Id);

        AddMalware(
            "keylogger_chrome_ext.crx", 256_000,
            "d6e4f5a6b7c8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4",
            71.3, 7.19, RiskLevel.High,
            "High (score 71.3)",
            new() { "Risk score exceeds High threshold", "Obfuscated JavaScript detected", "Browser credential harvester patterns" },
            cases[0].Id);

        AddMalware(
            "spearphish_macro.xlsm", 180_000,
            "e7f5a6b7c8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5",
            58.7, 6.93, RiskLevel.Medium,
            "Medium (score 58.7)",
            new() { "Obfuscated VBA macro detected", "Suspicious PowerShell download cradle", "Auto-execute on workbook open" },
            cases[3].Id);

        AddMalware(
            "sqli_webshell.php", 12_800,
            "f8a6b7c8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5a6",
            62.1, 6.55, RiskLevel.Medium,
            "Medium (score 62.1)",
            new() { "PHP webshell pattern detected", "Base64 encoded payload", "Remote command execution capability" },
            cases[4].Id);

        AddMalware(
            "wifi_capture_util.exe", 98_304,
            "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2",
            34.2, 5.88, RiskLevel.Low,
            "Low (score 34.2)",
            new() { "Packet sniffing library linked", "No obvious static indicators beyond network capture capability" },
            null);

        AddMalware(
            "system_health_check.sh", 4_096,
            "b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3",
            8.5, 3.21, RiskLevel.Clean,
            "Clean (score 8.5)",
            new() { "No obvious static indicators" },
            null);

        await context.MalwareAnalyses.AddRangeAsync(malwareItems);
        await context.SaveChangesAsync();

        logger.LogInformation(
            "Synthetic data seeded: {Cases} cases, {Evidence} evidence, {Custody} custody records, {Malware} malware analyses.",
            cases.Count, evidence.Count, custody.Count, malwareItems.Count);
    }
}
