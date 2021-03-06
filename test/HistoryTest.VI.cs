﻿using Microsoft.PowerShell;
using Xunit;

namespace Test
{
    // Disgusting language hack to make it easier to read a sequence of keys.
    using _ = Keys;

    public partial class ReadLine
    {
        [Fact]
        public void ViHistory()
        {
            TestSetup(KeyMode.Vi);

            // No history
            SetHistory();
            Test("", Keys(_.UpArrow, _.DownArrow));

            SetHistory("000", "001", "002", "003", "004");
            Test("004", Keys(_.UpArrow));

            SetHistory("000", "001", "002", "003", "004");
            Test("002", Keys(_.UpArrow, _.UpArrow, _.UpArrow));

            SetHistory("000", "001", "002", "003", "004");
            Test("004", Keys(_.UpArrow, _.UpArrow, _.DownArrow));

            SetHistory("000", "001", "002", "003", "004");
            Test("004", Keys(_.Escape, _.UpArrow));

            SetHistory("000", "001", "002", "003", "004");
            Test("002", Keys(_.Escape, _.UpArrow, _.UpArrow, _.UpArrow));

            SetHistory("000", "001", "002", "003", "004");
            Test("004", Keys(_.Escape, _.UpArrow, _.UpArrow, _.DownArrow));

            // For defect lzybkr/PSReadLine #571
            SetHistory("000", "001", "002", "003", "004");
            Test("004", Keys(_.Escape, "jk", _.Escape, "0C", _.UpArrow, _.Escape));

            SetHistory("000", "001", "002", "003", "004");
            Test("003", Keys(_.Escape, "kk"));

            SetHistory("000", "001", "002", "003", "004");
            Test("004", Keys(_.Escape, "kkj"));

            SetHistory("000", "001", "002", "003", "004");
            Test("003", Keys(_.Escape, "--"));

            SetHistory("000", "001", "002", "003", "004");
            Test("003", Keys(_.Escape, "---+"));
        }

        [Fact]
        public void ViSearchHistory()
        {
            TestSetup( KeyMode.Vi );

            // Clear history in case the above added some history (but it shouldn't)
            SetHistory();
            Test( " ", Keys( ' ', _.UpArrow, _.DownArrow ) );

            SetHistory( "dosomething", "this way", "that way", "anyway", "no way", "yah way" );

            Test( "dosomething", Keys(
                _.Escape, _.Slash, "some", _.Enter, CheckThat( () => AssertLineIs( "dosomething" ) ),
                _.Question, "yah", _.Enter, CheckThat( () => AssertLineIs( "yah way" ) ),
                _.Slash, "some", _.Enter, 'h'   // need 'h' here to avoid bogus failure
                ) );

            SetHistory("dosomething", "this way", "that way", "anyway", "no way", "yah way");

            Test("dosomething", Keys(
                _.Escape, _.CtrlR, "some", _.Enter, CheckThat(() => AssertLineIs("dosomething")),
                _.CtrlS, "yah", _.Enter, CheckThat(() => AssertLineIs("yah way")),
                _.CtrlR, "some", _.Enter, 'h'   // need 'h' here to avoid bogus failure
                ));

            SetHistory("dosomething", "this way", "that way", "anyway", "no way", "yah way");

            Test("dosomething", Keys(
                _.CtrlR, "some", _.Enter, CheckThat(() => AssertLineIs("dosomething")),
                _.CtrlS, "yah", _.Enter, CheckThat(() => AssertLineIs("yah way")),
                _.CtrlR, "some", _.Enter, _.Escape  // new esc here to avoid bogus failure
                ));
        }

        [Fact]
        public void ViHistoryRepeat()
        {
            TestSetup( KeyMode.Vi );
            // Clear history in case the above added some history (but it shouldn't)
            SetHistory();
            Test( " ", Keys( ' ', _.UpArrow, _.DownArrow ) );

            SetHistory( "dosomething", "this way", "that way", "anyway", "no way", "yah way" );
            Test( "anyway", Keys(
                _.Escape, _.Slash, "way", _.Enter, CheckThat( () => AssertLineIs( "yah way" ) ),
                "nn", CheckThat( () => AssertLineIs( "anyway" ) ),
                "N", CheckThat( () => AssertLineIs( "no way" ) ),
                "N", CheckThat( () => AssertLineIs( "yah way" ) ),
                "nn"
                ) );

            Test("anyway", Keys(
                _.Escape, _.Slash, "way", _.Tab, CheckThat(() => AssertLineIs("anyway")),
                "nnnn", CheckThat(() => AssertLineIs("that way")),
                "N", CheckThat(() => AssertLineIs("anyway")),
                "N", CheckThat(() => AssertLineIs("no way")),
                "n"
                ));

        }

        [Fact]
        public void ViMovementAfterHistory()
        {
            TestSetup(KeyMode.Vi);
            PSConsoleReadLine.SetOptions(new SetPSReadLineOption { HistorySearchCursorMovesToEnd = true });

            SetHistory("abc def ghi", "012 456 890");

            Test("012 456 890", Keys(
                _.Escape, "k", CheckThat(() => AssertCursorLeftIs(10))
                ));
        }

        [Fact]
        public void ViChangeAfterHistory()
        {
            TestSetup(KeyMode.Vi);

            SetHistory("abc def ghi", "012 456 890");

            Test("xyz", Keys(
                _.Escape, "kj", CheckThat(() => AssertLineIs("")),
                "clxyz", _.Escape
                ));
        }

        [Fact]
        public void ViAppendAfterHistory()
        {
            TestSetup(KeyMode.Vi);

            SetHistory("abc def ghi", "012 456 890");

            Test("xyz", Keys(
                _.Escape, "kj", CheckThat(() => AssertLineIs("")),
                "axyz", _.Escape
                ));

            Test("xyz", Keys(
                _.Escape, "kj", CheckThat(() => AssertLineIs("")),
                "Axyz", _.Escape
                ));
        }

        [Fact]
        public void ViHistoryCursorPosition()
        {
            TestSetup(KeyMode.Vi);
            PSConsoleReadLine.SetOptions(new SetPSReadLineOption { HistorySearchCursorMovesToEnd = false });

            SetHistory("abc def ghi", "012 456 890");

            Test("012 456 890", Keys(
                _.Escape, "k", CheckThat(() => AssertLineIs("012 456 890")),
                CheckThat(() => AssertCursorLeftIs(0))
                ));

        }
    }
}
