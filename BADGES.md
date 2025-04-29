# Badges for ResilientHttpClient

<!-- Code Coverage Badge (example: Coveralls, Codecov, or local badge) -->
![Coverage](https://img.shields.io/badge/coverage-100%25-brightgreen?style=flat-square)

<!-- License Badge -->
![License](https://img.shields.io/badge/License-MIT-blue.svg?style=flat-square)

<!-- .NET Standard Support Badge -->
![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.1-blueviolet?style=flat-square)

---

## How to Use in README.md
Copy and paste the following at the very top of your README.md, right under the project title:

```
![Coverage](https://img.shields.io/badge/coverage-100%25-brightgreen?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-blue.svg?style=flat-square)
![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.1-blueviolet?style=flat-square)
```

---

## Notes
- **Code coverage**: Since you have comprehensive xUnit and Moq tests, you can use a tool like Coverlet or ReportGenerator to generate a real badge. The above is a static badge for illustration. If you upload to a service like Codecov or Coveralls, replace the badge URL accordingly.
- **License**: MIT, as per your README.
- **.NET Standard**: The library targets .NET Standard 2.1, as noted in your docs.

If you want these badges automatically updated, consider integrating with a CI service and a badge provider (Codecov, Shields.io, etc.).
