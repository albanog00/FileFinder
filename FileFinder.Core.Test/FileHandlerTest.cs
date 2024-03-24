using FileFinder.Core.Handler;

namespace FileFinder.Core.Test
{
    public class FileHandlerTest
    {
        public string[] files = [
            "a.foo",
            "b.f",
            "c.exe",
            "d.foobar",
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
                "d.foobar",
                "a/b.foo",
                "a/b/c.foo"
            ];

            FileHandler fileHandler = new(null, "foo");
            List<string> actual = [];
            actual.AddRange(files
                    .Where(fileHandler.Validate));

            Assert.Equal(expect, actual.ToArray());
        }

        [Fact]
        public void Validate_ShouldGetAllFilesPathThatContainsProvidedName()
        {
            string[] expect = [
                "a.foo",
                "d.foobar",
                "a/b.exe",
                "a/b.foo",
                "a/b/c.foo",
                "b/c/d.cas.md"
            ];

            FileHandler fileHandler = new("a");
            List<string> actual = [];
            actual.AddRange(files
                .Where(fileHandler.Validate));

            Assert.Equal(expect, actual.ToArray());
        }

        [Fact]
        public void Validate_ShouldGetAllFilesPathThatContainsProvidedNameAndExtension()
        {
            string[] expect = [
                "a/b.exe",
            ];

            FileHandler fileHandler = new("a", "exe");
            List<string> actual = [];
            actual.AddRange(files
                .Where(fileHandler.Validate));

            Assert.Equal(expect, actual.ToArray());
        }

        [Fact]
        public void Validate_ShouldGetAllFilesPathWhereIsExactFileNameAndContainsExtension()
        {
            string[] expect = [
                "b.f",
                "a/b.exe",
                "a/b.foo",
            ];

            FileHandler fileHandler = new("b", null, true);
            List<string> actual = [];
            actual.AddRange(files
                .Where(fileHandler.Validate));

            Assert.Equal(expect, actual.ToArray());
        }

        [Fact]
        public void GetFileExtension_ShouldGetAllFilesExtensions()
        {
            string[] expect = [
                ".foo",
                ".f",
                ".exe",
                ".foobar",
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
                "b.f",
                "c.exe",
                "d.foobar",
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
