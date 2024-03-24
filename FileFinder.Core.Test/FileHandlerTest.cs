using FileFinder.Core.Handler;

namespace FileFinder.Core.Test
{
    public class FileHandlerTest
    {
        public string[] files = [
            "a.foo",
            "b.foo",
            "c.exe",
            "a/b.exe",
            "a/b.foo",
            "a/b/c.foo",
            "b/c/d.cas.md"
        ];

        [Fact]
        public void Validate_ShouldGetAllFilesWithProvidedExtension()
        {
            string[] expect = [
                "a.foo",
                "b.foo",
                "a/b.foo",
                "a/b/c.foo"
            ];

            FileHandler fileChecker = new(null, "foo");
            List<string> actual = [];
            actual.AddRange(files
                    .Where(fileChecker.Validate));

            Assert.Equal(expect, actual.ToArray());
        }

        [Fact]
        public void Validate_ShouldGetAllFilesPathThatContainsProvidedName()
        {
            string[] expect = [
                "a.foo",
                "a/b.exe",
                "a/b.foo",
                "a/b/c.foo",
                "b/c/d.cas.md"
            ];

            FileHandler fileChecker = new("a");
            List<string> actual = [];
            actual.AddRange(files
                    .Where(fileChecker.Validate));

            Assert.Equal(expect, actual.ToArray());
        }

        [Fact]
        public void Validate_ShouldGetAllFilesPathThatContainsProvidedNameAndExtension()
        {
            string[] expect = [
                "a/b.exe",
            ];

            FileHandler fileChecker = new("a", "exe");
            List<string> actual = [];
            actual.AddRange(files
                    .Where(fileChecker.Validate));

            Assert.Equal(expect, actual.ToArray());
        }

        [Fact]
        public void GetFileExtension_ShouldGetAllFilesExtensions()
        {
            string[] expect = [
                ".foo",
                ".foo",
                ".exe",
                ".exe",
                ".foo",
                ".foo",
                ".cas.md"
            ];

            string[] actual = new string[files.Length];
            for (int i = 0; i < files.Length; ++i)
            {
                actual[i] = FileHandler.GetFileExtension(files[i]);
            }

            Assert.Equal(expect, actual);
        }

        [Fact]
        public void GetFileName_ShouldGetAllFilesName()
        {
            string[] expect = [
                "a.foo",
                "b.foo",
                "c.exe",
                "b.exe",
                "b.foo",
                "c.foo",
                "d.cas.md"
            ];

            string[] actual = new string[files.Length];
            for (int i = 0; i < files.Length; ++i)
            {
                actual[i] = FileHandler.GetFileName(files[i]);
            }

            Assert.Equal(expect, actual);
        }

    }
}
