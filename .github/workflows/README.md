# CI/CD Pipeline Documentation

## Overview

The EmailService project uses GitHub Actions for automated Continuous Integration and Continuous Deployment with semantic versioning and Docker container publishing.

## Workflows

### 1. CI Workflow (`ci.yml`)

**Trigger:** Pull requests to `main` branch

**Steps:**
1. **Checkout** - Fetches code with full history
2. **.NET Setup** - Configures .NET 10.0 runtime
3. **Restore** - Restores NuGet dependencies
4. **Build** - Compiles all projects in Release configuration
5. **Unit Tests** - Runs EmailService.Tests and EmailPublisher.Tests
6. **Code Quality** - Treats warnings as errors for production readiness
7. **Test Reporting** - Publishes test results in GitHub UI

**File Paths Monitored:**
- `email-service/**` - Service code changes
- `email-publisher-client/**` - Publisher client changes
- `.github/workflows/ci.yml` - Workflow changes

### 2. CD Workflow (`cd.yml`)

**Trigger:** 
- Push to `main` branch (after PR merge)
- Manual trigger via workflow_dispatch

**Steps:**
1. **Checkout** - Fetches code with full history
2. **.NET Build** - Builds in Release configuration
3. **Tests** - Runs unit tests before Docker build
4. **Docker Setup** - Configures Docker Buildx for multi-platform builds
5. **Registry Login** - Authenticates to GitHub Container Registry (GHCR)
6. **Metadata Extraction** - Generates Docker tags:
   - Branch name (e.g., `main`)
   - Semantic version tags (e.g., `1.0.0`, `1.0`, `1`)
   - Git SHA (short commit hash)
   - `latest` tag for main branch
7. **Docker Build & Push** - Builds and pushes to GHCR with layer caching
8. **Release Creation** - Generates semantic version and GitHub Release

**Environment Variables:**
- `REGISTRY`: `ghcr.io` (GitHub Container Registry)
- `IMAGE_NAME`: `${{ github.repository }}/email-service`

### 3. Tag and Push Action (`tag-and-push/action.yml`)

Custom composite action for semantic versioning and release management.

**Inputs:**
- `github-token` - GitHub token for creating releases and tags
- `image-tag` - Docker image tag from CD workflow

**Steps:**
1. **Determine Version** - Calculates next semantic version:
   - Fetches latest git tag
   - Increments patch version (e.g., v1.0.0 → v1.0.1)
2. **Create Tag** - Creates and pushes git tag
3. **Create Release** - Generates GitHub Release with:
   - Version number in release name
   - Docker image reference
   - Link to commit history

## Setup Instructions

### Prerequisites

1. **GitHub Repository** - Project must be on GitHub
2. **Secrets Configuration** - No additional secrets needed (uses GITHUB_TOKEN)
3. **Branch Protection** - Set main branch to require PR reviews before merge

### Configuration

#### 1. Enable GitHub Container Registry (GHCR)

- GitHub automatically enables GHCR for all repositories
- Token permissions are controlled via workflow YAML (`contents: read, packages: write`)

#### 2. Set Repository Variables (Optional)

In GitHub repo Settings → Secrets and variables → Actions:

```
# No required secrets - uses built-in GITHUB_TOKEN
# Optional: Add custom registry or notifications
```

#### 3. Configure Branch Protection

In Settings → Branches → Add rule for `main`:
- ✅ Require pull request reviews before merging
- ✅ Require status checks to pass (CI workflow)
- ✅ Require branches to be up to date before merging

### Workflow Files Location

```
.github/
└── workflows/
    ├── ci.yml
    ├── cd.yml
    └── tag-and-push/
        └── action.yml
```

## Usage

### Development: Creating a Pull Request

```bash
git checkout -b feature/my-feature
# Make changes
git commit -m "Add new feature"
git push origin feature/my-feature
```

**GitHub Actions automatically:**
1. Runs CI workflow on PR creation
2. Compiles code
3. Runs tests
4. Reports results in PR checks

✅ PR will show passing/failing status before merge

### Production: Merging to Main

```bash
# In GitHub UI or CLI:
# Merge PR to main
```

**GitHub Actions automatically:**
1. Runs CD workflow after merge
2. Builds and tests Docker image
3. Pushes to GHCR with multiple tags
4. Creates semantic version tag (e.g., v1.0.1)
5. Generates GitHub Release

✅ Release appears in repository Releases page

### Manual Deployment

To trigger CD workflow without pushing:

1. Go to GitHub repo → Actions → CD workflow
2. Click "Run workflow"
3. Select branch (main)
4. Click "Run workflow"

## Docker Image Naming

Images are tagged with semantic versions and branch information:

```
ghcr.io/owner/repo/email-service:v1.0.1     # Semantic version
ghcr.io/owner/repo/email-service:1.0        # Major.minor
ghcr.io/owner/repo/email-service:1          # Major only
ghcr.io/owner/repo/email-service:main-abc123# Branch + commit SHA
ghcr.io/owner/repo/email-service:latest     # Latest (main branch only)
```

## Pulling Docker Images

### From GitHub Container Registry

```bash
# Login (one-time)
docker login ghcr.io -u USERNAME -p <PAT>

# Pull image
docker pull ghcr.io/owner/repo/email-service:latest

# Run container
docker run -d \
  -e SMTP__HOST=mailhog \
  -e RABBITMQ__HOST=rabbitmq \
  ghcr.io/owner/repo/email-service:latest
```

## Monitoring

### View Workflow Runs

1. Go to repository → Actions tab
2. Click on workflow name (CI or CD)
3. View detailed logs for each step

### Troubleshooting

**CI Workflow Failures:**
- Check test output in GitHub Actions logs
- Verify .NET version compatibility
- Check for hardcoded paths or environment assumptions

**CD Workflow Failures:**
- Verify Docker build succeeds locally: `docker build -f email-service/src/EmailService.Api/Dockerfile email-service`
- Check GHCR authentication (verify GITHUB_TOKEN permissions)
- Review Docker layer cache issues

**Release Creation Issues:**
- Verify git credentials configured in action
- Check for existing tags (may cause conflicts)
- Ensure write permissions to repository

## Best Practices

### 1. Branch Strategy

- **main** - Production-ready code, protected branch
- **develop** - Integration branch (optional)
- **feature/*** - Feature branches, always create PR

### 2. Commit Messages

Use conventional commits for automated release notes:

```
feat: Add email retry logic
fix: Handle RabbitMQ connection timeout
docs: Update API documentation
chore: Update dependencies
```

### 3. Testing

- All tests must pass before merge
- Add tests for new features
- Keep test execution time under 5 minutes

### 4. Code Quality

- Warnings treated as errors in Release build
- No TODO comments in production code
- Follow C# naming conventions (PascalCase for public members)

## Advanced Configuration

### Customize Docker Build Arguments

Edit `cd.yml` build-push-action step:

```yaml
- name: Build and push Docker image
  uses: docker/build-push-action@v5
  with:
    context: email-service
    file: email-service/src/EmailService.Api/Dockerfile
    build-args: |
      VERSION=${{ steps.version.outputs.version_no_v }}
      BUILD_DATE=$(date -u +'%Y-%m-%dT%H:%M:%SZ')
    push: true
    tags: ${{ steps.meta.outputs.tags }}
    labels: ${{ steps.meta.outputs.labels }}
```

### Add Slack/Email Notifications

Add to `cd.yml` after release creation:

```yaml
- name: Notify Slack
  if: success()
  uses: slackapi/slack-github-action@v1
  with:
    webhook-url: ${{ secrets.SLACK_WEBHOOK }}
    payload: |
      {
        "text": "Email Service v${{ steps.version.outputs.version }} released!",
        "blocks": [...]
      }
```

### Publish to Docker Hub

Add step before push in `cd.yml`:

```yaml
- name: Login to Docker Hub
  uses: docker/login-action@v3
  with:
    username: ${{ secrets.DOCKERHUB_USERNAME }}
    password: ${{ secrets.DOCKERHUB_TOKEN }}

# In build-push-action, add: docker.io/${{ secrets.DOCKERHUB_REPO }}/email-service to tags
```

## Related Documentation

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Docker Build and Push Action](https://github.com/docker/build-push-action)
- [Semantic Versioning](https://semver.org/)
- [Conventional Commits](https://www.conventionalcommits.org/)
