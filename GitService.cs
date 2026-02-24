using System.Diagnostics;
using System.Text;
using cp.Models;

namespace cp;

public class GitService
{
    private readonly string _repoPath;

    public GitService(string repoPath)
    {
        _repoPath = repoPath;
    }

    // ── Basic checks ────────────────────────────────────────────────────────

    public bool IsGitRepo()
    {
        var result = Run("rev-parse", "--is-inside-work-tree");
        return result.Success && result.Output.Trim() == "true";
    }

    public string CurrentBranch()
    {
        var result = Run("branch", "--show-current");
        return result.Success ? result.Output.Trim() : "HEAD";
    }

    public string[] AllBranches()
    {
        var result = Run("branch", "-a", "--format=%(refname:short)");
        if (!result.Success) return [];
        return result.Output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(b => b.Trim().TrimStart('*').Trim())
            .Where(b => b.Length > 0)
            .Distinct()
            .ToArray();
    }

    // ── Commits ──────────────────────────────────────────────────────────────

    public List<CommitInfo> GetCommits(string branch, int limit = 50)
    {
        // format: <hash>|<short>|<author>|<date>|<subject>
        var result = Run("log", branch,
            $"--max-count={limit}",
            "--pretty=format:%H|%h|%an|%ad|%s",
            "--date=short");

        if (!result.Success || string.IsNullOrWhiteSpace(result.Output))
            return [];

        var commits = new List<CommitInfo>();
        foreach (var line in result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('|', 5);
            if (parts.Length == 5)
                commits.Add(new CommitInfo(parts[0], parts[1], parts[2], parts[3], parts[4]));
        }
        return commits;
    }

    // ── Branch operations ────────────────────────────────────────────────────

    public bool BranchExists(string branch)
    {
        var result = Run("branch", "--list", branch);
        return result.Success && !string.IsNullOrWhiteSpace(result.Output);
    }

    public GitResult CheckoutExisting(string branch)   => Run("checkout", branch);
    public GitResult CheckoutNew(string branch)        => Run("checkout", "-b", branch);

    // ── Cherry-pick ──────────────────────────────────────────────────────────

    public GitResult CherryPick(string hash)           => Run("cherry-pick", hash);
    public GitResult CherryPickContinue()              => Run("cherry-pick", "--continue");
    public GitResult CherryPickAbort()                 => Run("cherry-pick", "--abort");
    public GitResult CherryPickSkip()                  => Run("cherry-pick", "--skip");

    public string[] ConflictedFiles()
    {
        var result = Run("diff", "--name-only", "--diff-filter=U");
        if (!result.Success || string.IsNullOrWhiteSpace(result.Output))
            return [];
        return result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    }

    public string GetConflictDiff(string file)
    {
        var result = Run("diff", "--", file);
        return result.Output;
    }

    public GitResult StageAll()                        => Run("add", "-A");

    // ── Runner ───────────────────────────────────────────────────────────────

    public GitResult Run(params string[] args)
    {
        var psi = new ProcessStartInfo("git")
        {
            WorkingDirectory        = _repoPath,
            RedirectStandardOutput  = true,
            RedirectStandardError   = true,
            UseShellExecute         = false,
        };

        // git cherry-pick --continue needs a terminal for the editor; skip the
        // editor by passing GIT_EDITOR=true (a no-op command that returns 0).
        psi.EnvironmentVariables["GIT_EDITOR"] = "true";

        foreach (var a in args) psi.ArgumentList.Add(a);

        using var proc = Process.Start(psi)!;

        var stdoutTask = proc.StandardOutput.ReadToEndAsync();
        var stderrTask = proc.StandardError.ReadToEndAsync();
        proc.WaitForExit();

        var stdout = stdoutTask.Result;
        var stderr = stderrTask.Result;

        return new GitResult(proc.ExitCode == 0, stdout, stderr, proc.ExitCode);
    }
}

public record GitResult(bool Success, string Output, string Error, int ExitCode)
{
    public string CombinedOutput => string.IsNullOrWhiteSpace(Error)
        ? Output
        : string.IsNullOrWhiteSpace(Output) ? Error : $"{Output}\n{Error}";
}
