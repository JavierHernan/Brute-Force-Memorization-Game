using System;
using System.Collections.Generic;
using System.Linq;

// ─── Protocol model ──────────────────────────────────────────────────────────
record Protocol(string Abbr, string? Full, int[] Ports);

class Program
{
    // ─── Protocol data ───────────────────────────────────────────────────────
    static readonly List<Protocol> Protocols = new()
    {
        new("HTTP", "HyperText Transfer Protocol", new[] { 80 }),
        new("HTTPS", "HyperText Transfer Protocol Secure", new[] { 443 }),
        new("SMTP", "Simple Mail Transfer Protocol", new[] { 25 }),
        new("SMTPS", "Simple Mail Transfer Protocol Secure", new[] { 465, 587 }),
        new("POP3", "Post Office Protocol version 3", new[] { 110 }),
        new("POP3S", "Post Office Protocol version 3 Secure", new[] { 995 }),
        new("IMAP", "Internet Message Access Protocol", new[] { 143 }),
        new("IMAPS", "Internet Message Access Protocol Secure", new[] { 993 }),
        new("SSH", "Secure Shell", new[] { 22 }),
        new("SFTP", "SSH File Transfer Protocol", new[] { 22 }),
        new("Telnet", "Telecommunication Network", new[] { 23 }),
        new("RDP", "Remote Desktop Protocol", new[] { 3389 }),
        new("FTP", "File Transfer Protocol", new[] { 20, 21 }),
        new("TFTP", "Trivial File Transfer Protocol", new[] { 69 }),
        new("DNS", "Domain Name System", new[] { 53 }),
        new("DHCP", "Dynamic Host Configuration Protocol", new[] { 67, 68 }),
        new("LDAP", "Lightweight Directory Access Protocol", new[] { 389 }),
        new("LDAPS", "Lightweight Directory Access Protocol Secure", new[] { 636 }),
        new("SQL Service (MSSQL)", "Microsoft SQL Server", new[] { 1433 }),
        new("SQL Service (MySQL)", "MySQL Server", new[] { 3306 }),
        new("SNMP", "Simple Network Management Protocol", new[] { 161, 162 }),
        new("SysLog", "System Logging Protocol", new[] { 514 }),
        new("NTP", "Network Time Protocol", new[] { 123 }),
        new("SIP", "Session Initiation Protocol", new[] { 5060, 5061 }),
        new("H.323", null, new[] { 1720 }),
        new("SMB and CIFS", "Server Message Block / Common Internet File System", new[] { 445 }),
        new("NetBIOS and NetBT", "Network Basic Input/Output System", new[] { 137, 138, 139 }),
    };

    static readonly Random Rng = new();

    // ─── Helpers ─────────────────────────────────────────────────────────────
    static List<T> Shuffle<T>(IEnumerable<T> source)
    {
        var list = source.ToList();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }

    static List<int> NormalizePorts(string input)
    {
        return input
            .Split(new[] { '/', '-', ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out int n) ? n : -1)
            .Where(n => n > 0 && n <= 65535)
            .Distinct()
            .OrderBy(n => n)
            .ToList();
    }

    static bool PortsEqual(List<int> a, int[] b)
    {
        var sortedB = b.OrderBy(x => x).ToArray();
        if (a.Count != sortedB.Length) return false;
        for (int i = 0; i < a.Count; i++)
            if (a[i] != sortedB[i]) return false;
        return true;
    }

    static string CanonicalPorts(int[] ports) =>
        string.Join("/", ports.OrderBy(p => p));

    static string DisplayName(Protocol p) =>
        p.Full is null ? p.Abbr : $"{p.Abbr} - {p.Full}";

    // ─── Core ask function ───────────────────────────────────────────────────
    // Returns: true = correct, false = incorrect (but acknowledged), null = quit
    static bool? AskProtocol(Protocol proto, bool isReview = false)
    {
        var expected = proto.Ports.OrderBy(p => p).ToArray();
        var canon = CanonicalPorts(expected);
        var name = DisplayName(proto);

        Console.WriteLine();
        Console.WriteLine(isReview
            ? $"REVIEW → Protocol: {name}"
            : $"Protocol: {name}");
        Console.WriteLine("Enter the port number(s) (separate multiple with /, any order is fine):");

        Console.Write("> ");
        string? answer = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(answer) || answer.Equals("quit", StringComparison.OrdinalIgnoreCase))
            return null;

        var userPorts = NormalizePorts(answer);

        if (PortsEqual(userPorts, expected))
        {
            if (isReview)
                Console.WriteLine("  ✓ Correct");
            return true;
        }

        // Incorrect
        Console.WriteLine($"\n✗ Incorrect. The correct answer is: {canon}");

        // Force the user to type the correct answer
        while (true)
        {
            Console.Write("Type the correct port(s) to acknowledge: ");
            string? ack = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(ack) || ack.Equals("quit", StringComparison.OrdinalIgnoreCase))
                return null;

            if (PortsEqual(NormalizePorts(ack), expected))
                break;

            Console.WriteLine("  That does not match. Please type the correct port(s).");
        }

        Console.Write("Press Enter to continue...");
        Console.ReadLine();
        return false;
    }

    // ─── Review helper ───────────────────────────────────────────────────────
    static bool DoReview(List<Protocol> mastered)
    {
        if (mastered.Count == 0) return true;

        Console.WriteLine("\n────────────────────────────────────────");
        Console.WriteLine("  Reviewing previously mastered protocols");
        Console.WriteLine("  (shuffled order)");
        Console.WriteLine("────────────────────────────────────────");

        foreach (var proto in Shuffle(mastered))
        {
            var result = AskProtocol(proto, isReview: true);
            if (result is null) return false; // quit
        }

        Console.WriteLine("\n── Review complete ──");
        return true;
    }

    // ─── Main game loop ──────────────────────────────────────────────────────
    static void Main()
    {
        // Enable UTF-8 so ✓ ✗ and box-drawing characters display correctly
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("╔══════════════════════════════════════════════════════╗");
        Console.WriteLine("║     Ports & Protocols Memorization Game              ║");
        Console.WriteLine("║     Brute-force learning through repetition          ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("• Protocols are presented one at a time in random order.");
        Console.WriteLine("• Type the port(s) using / as separator (e.g. 20/21).");
        Console.WriteLine("• On a miss you must type the correct answer, then re-answer");
        Console.WriteLine("  every previously mastered protocol before retrying.");
        Console.WriteLine("• Score (streak) resets on any incorrect answer.");
        Console.WriteLine("• Type \"quit\" at any prompt to exit.\n");

        var remaining = new List<Protocol>(Protocols);
        var mastered = new List<Protocol>();
        int streak = 0;
        Protocol? pendingRetry = null;
        int total = Protocols.Count;

        while (mastered.Count < total)
        {
            Protocol proto;

            if (pendingRetry is not null)
            {
                proto = pendingRetry;
            }
            else if (remaining.Count > 0)
            {
                int idx = Rng.Next(remaining.Count);
                proto = remaining[idx];
            }
            else break;

            if (pendingRetry is not null)
                Console.WriteLine("\n>>> Retrying the protocol you previously missed <<<");

            var result = AskProtocol(proto);

            if (result is null)
            {
                Console.WriteLine("\nExiting game. Goodbye!");
                return;
            }

            if (result == true)
            {
                if (pendingRetry is not null)
                    pendingRetry = null;
                else
                    remaining.Remove(proto);

                if (!mastered.Contains(proto))
                    mastered.Add(proto);

                streak++;
                Console.WriteLine($"\n✓ Correct!  Streak: {streak}  |  Mastered: {mastered.Count}/{total}");
            }
            else
            {
                streak = 0;
                Console.WriteLine($"\nStreak reset to 0.  Mastered so far: {mastered.Count}/{total}");

                if (!DoReview(mastered))
                {
                    Console.WriteLine("\nExiting game. Goodbye!");
                    return;
                }

                pendingRetry = proto;
                Console.WriteLine("\nNow retrying the protocol you missed...\n");
            }
        }

        if (mastered.Count == total)
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════╗");
            Console.WriteLine("║  🎉  Congratulations! You mastered every protocol!   ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════╝");
            Console.WriteLine($"Final streak: {streak}");
            Console.WriteLine($"Total protocols: {total}");
        }
    }
}