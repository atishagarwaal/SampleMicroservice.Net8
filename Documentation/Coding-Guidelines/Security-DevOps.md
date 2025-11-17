# Security & DevOps

## Overview

This document covers security best practices, secret management, rate limiting, and CI/CD pipeline discipline.

## 🔐 Security by Default

Security should be an instinct, not a checklist.

```csharp
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = false;
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true
            };
    });
```

✅ Validate all input
✅ Use parameterized queries
✅ Enforce HTTPS
✅ Principle of least privilege for services

---

## 🔐 Secret Management

* Store secrets in **Azure Key Vault**, **AWS Secrets Manager**, or environment variables.
* Never commit `.json` files with passwords or tokens.
* Rotate credentials regularly.

---

## 🧮 Rate Limiting & API Protection

Add rate limiting and proper status codes.

```csharp
builder.Services.AddRateLimiter(o =>
{
    o.AddFixedWindowLimiter("default", options =>
    {
        options.PermitLimit = 100;
        options.Window = TimeSpan.FromMinutes(1);
    });
});
```

✅ Return `429 Too Many Requests` when throttled.
✅ Log client IP and API key on rate limit events.

---

## 🧩 CI/CD Pipelines and DevOps Discipline

### Git Hygiene

✅ One logical change per commit
✅ Keep PRs < 400 lines
✅ Rebase > merge
✅ Always write tests
✅ Integrate with SonarQube or CodeQL

### Branch Naming Convention

All branches should follow the format `<story-type>/<story-number>_<descriptive-text>`

Where:
- story-type: "feature", "bugfix", etc.
- story-number: Jira story number, ex. SS-123456
- descriptive-text: (Optional) description of the story. Try to keep this short as possible

Examples:
* `feature/SS-123456_FeatureWork`
* `feature/SS-987654`
* `bugfix/SS-24680_ExceptionFix`

### Commit Message Standards

**Subject Line:**
- Use the **imperative mood** (e.g., Add, Fix, Refactor not Added, Fixes).
- Limit the subject to **50 characters**.
- **Capitalize** the subject line.
- Do **not** end the subject line with a period.

**Body (Optional but Recommended):**
- Separate the subject from the body with a blank line.
- Explain the **"what" and "why"**, not the "how". The code itself shows the "how".
- Wrap the body at **72 characters**.

**Use Conventional Commits:**
- Prefix your commit message with a type, such as feat:, fix:, docs:, style:, refactor:, test:, or chore:.
- Example: `feat(auth): Add password reset endpoint`

---

## Related Documentation

- [Solution Patterns](./Solution-Patterns.md) - Build configuration patterns
- [Architecture Principles](./Architecture-Principles.md) - Security architecture
- [Testing Guidelines](./Testing-Guidelines.md) - CI/CD testing practices

