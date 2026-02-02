Technical Architecture: Specular Path & CSP Enemy SpawnerProject: Damned Veil (2D Top-Down Shooter)1. High-Level ArchitectureSistem ini terdiri dari tiga modul utama yang bekerja secara sekuensial:Path Finder (Specular Path): Bertanggung jawab atas kalkulasi geometri dan fisika pantulan.Constraint Solver (CSP): Bertanggung jawab atas logika validasi dan seleksi posisi.Level Manager (Orchestrator): Menggabungkan kedua modul di atas dan melakukan instansiasi objek (spawning).2. Data Structures (Models)Definisi struktur data yang harus digunakan agar antar-modul dapat berkomunikasi.[System.Serializable]
public struct TrajectoryPoint {
    public Vector2 Position;
    public Vector2 Normal; // Normal dinding jika ini titik pantul
    public int BounceIndex; // Pantulan ke-berapa (0 = source)
}

[System.Serializable]
public class SpecularPathData {
    public List<TrajectoryPoint> PathPoints; // Titik-titik sudut lintasan
    public float TotalLength;
    public bool IsClosedLoop; // Apakah path valid mengenai target akhir (jika ada)
}

[System.Serializable]
public struct EnemySpawnData {
    public Vector2 Position;
    public int OnSegmentIndex; // Berada di segmen lintasan mana
}
3. Module Breakdown & Logic FlowModule A: Specular Path Generator (Image Source Method)Modul ini mensimulasikan lintasan peluru "sempurna" yang memantul.Input: PlayerPosition, LayerMask (Walls), MaxBounces (6).Algorithm (Image Source Method):Mulai dari posisi Player.Lakukan Physics2D.Raycast ke arah acak (atau arah terarah jika menggunakan target dummy).Saat menabrak dinding:Hitung titik pantul (Vector2.Reflect).Simpan titik tabrakan.Lanjutkan Raycast baru dari titik pantul tersebut.Ulangi hingga MaxBounces tercapai atau tidak ada tabrakan.Output: Objek SpecularPathData.Module B: CSP Solver (Validator)Modul ini menerima lintasan garis, lalu mencoba menempatkan titik-titik musuh di sepanjang garis tersebut sesuai aturan.Input: SpecularPathData, MinEnemies, PlayerPosition.Constraints (Berdasarkan Tabel 3.1):C1 (Distance): Jarak antar musuh > minEnemySpacing (mencegah penumpukan).C2 (Bounces): Total pantulan path harus <= 6 (sudah dihandle Module A, tapi divalidasi ulang).C3 (Safe Zone): Jarak musuh ke Player > safeZoneRadius (misal: 3 unit).C4 (Min Count): Total musuh yang ditempatkan >= minEnemyCount.Logic:Lakukan sampling titik di sepanjang garis lintasan (misal setiap 0.5 unit).Filter titik yang melanggar C3 (Safe Zone).Pilih titik secara acak dari domain yang tersisa.Saat menempatkan titik kedua dst, cek C1 (Distance) terhadap titik yang sudah ada.Jika jumlah titik valid < MinEnemies, return Fail.Output: List EnemySpawnData atau null (jika gagal).Module C: Level OrchestratorLoop:Panggil SpecularPathGenerator.GeneratePath().Masukkan hasil path ke CSPSolver.Solve().Jika Solver mengembalikan Valid:Instantiate Enemy Prefabs di posisi yang dihasilkan.Selesai (Stop Loop).Jika Solver mengembalikan Fail:Ulangi langkah 1 dengan sudut tembak (Raycast direction) yang berbeda.Lakukan hingga MaxAttempts (misal 100x). Jika limit tercapai, gunakan fallback (random spawn).4. Prompt untuk Agentic Tool (Copy-Paste ini ke AI Coding Tool)Gunakan prompt di bawah ini untuk memerintahkan AI membuat kode:Role: Anda adalah Senior Unity Developer yang ahli dalam Procedural Content Generation (PCG).Task: Implementasikan sistem Enemy Spawner untuk game 2D Top-Down Shooter bernama "Damned Veil". Sistem ini harus menjamin bahwa semua musuh yang di-spawn BISA dikalahkan dalam satu tembakan pantul (Ricochet Sweetspot).Requirements:Buat script SpecularPathGenerator.cs:Gunakan Physics2D.Raycast untuk mensimulasikan pantulan peluru.Input: Origin (Vector2), Direction (Vector2), MaxBounces (int).Output: List titik-titik lintasan (Vector2).Buat script CSPValidator.cs:Menerima List Vector2 (path) dari generator.Mencoba menempatkan n musuh di sepanjang garis tersebut.Wajib menerapkan Constraints berikut:Safe Zone: Musuh tidak boleh spawn dalam radius 3 unit dari Player.Spacing: Jarak antar musuh minimal 2 unit.Limit: Jangan spawn musuh di ujung akhir lintasan.Buat script LevelManager.cs:Melakukan loop while (!success && attempts < 100).Di dalam loop: Generate arah random -> Generate Path -> Coba Validate dengan CSP.Jika sukses: Spawn Prefab Musuh di titik valid dan gambar garis lintasan menggunakan LineRenderer atau OnDrawGizmos untuk debug.Context:Game ini menggunakan mekanik di mana satu peluru memantul bisa membunuh semua musuh yang dilewatinya. Jadi posisi musuh harus berada tepat di garis lintasan (Line Segment) yang dihasilkan.5. Diagram Alur Logika (Pseudocode Flow)FUNCTION SpawnEnemies():
    SET attempts = 0
    WHILE attempts < 100:
        // 1. Specular Path Phase
        angle = Random.Range(0, 360)
        path = SpecularPathGenerator.Simulate(playerPos, angle, maxBounces=6)
        
        IF path.Length < minPathLength THEN
            attempts++
            CONTINUE
        
        // 2. CSP Phase
        validPositions = []
        possiblePoints = SamplePointsAlongPath(path, resolution=0.5f)
        
        FOREACH point IN possiblePoints:
            IF Distance(point, playerPos) < SafeZoneRadius THEN SKIP
            
            validPositions.Add(point)
        
        // Cek Constraint Kepadatan & Jumlah
        finalEnemies = SelectRandomPoints(validPositions, minDistance=2.0f)
        
        IF finalEnemies.Count >= MinEnemiesRequired THEN
            // 3. Spawning Phase
            InstantiateEnemies(finalEnemies)
            RETURN Success
            
        attempts++

    PRINT "Failed to generate valid level via PCG"
