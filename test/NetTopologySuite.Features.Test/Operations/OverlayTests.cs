using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
using NetTopologySuite.Features.Operations;

namespace NetTopologySuite.Features.Test.Operations
{
    [TestFixture]
    public sealed class OverlayTests
    {
        IEnumerable<Feature> _inputData = new List<Feature>
        {

            new Feature
            {
                Geometry = new WKTReader().Read("POLYGON((-0.31108624647558 51.6069894245466,-0.143539765234802 51.6044591701841,-0.146547487850035 51.5305626910209,-0.313822811181368 51.5330862940173,-0.31108624647558 51.6069894245466))"),
                Attributes = new AttributesTable(new[]{ new KeyValuePair<string, object>("ID", 1) })
            },
            new Feature
            {
                Geometry = new WKTReader().Read("POLYGON((-0.143539752679269 51.6044591656482,0.051898556044701 51.601205166436,0.048477118693013 51.5251440536077,-0.146635743529052 51.5283892488822,-0.143539752679269 51.6044591656482))"),
                Attributes = new AttributesTable(new[]{ new KeyValuePair<string, object>("ID", 2) })
            },
            new Feature
            {
                Geometry = new WKTReader().Read("POLYGON((-0.313822798743524 51.5330862894274,-0.14654748802908 51.5305626866125,-0.149849994025209 51.4490579895643,-0.316827569123474 51.4515742811323,-0.313822798743524 51.5330862894274))"),
                Attributes = new AttributesTable(new[]{ new KeyValuePair<string, object>("ID", 3) })
            },
            new Feature
            {
                Geometry = new WKTReader().Read("POLYGON((0.044779371226155 51.4425621047187,-0.149981759235782 51.4457977744333,-0.146547475338766 51.5305626864295,0.048574665581212 51.5273172400195,0.044779371226155 51.4425621047187))"),
                Attributes = new AttributesTable(new[]{ new KeyValuePair<string, object>("ID", 4) })
            }
        };

        IEnumerable<Feature> _overlayData = new List<Feature>
        {
            new Feature
            {
                Geometry = new WKTReader().Read("POLYGON((-0.360881837589005 51.6305301590296,-0.20022850094338 51.6281773907915,-0.203189696115119 51.5531921294621,-0.363578970002872 51.5555386207141,-0.360881837589005 51.6305301590296))"),
                Attributes = new AttributesTable(new[]{ new KeyValuePair<string, object>("Name", "A") })
            },
            new Feature
            {
                Geometry = new WKTReader().Read("POLYGON((-0.080948994382577 51.6393371189262,-0.016333383800898 51.6382631619151,-0.022613931268813 51.4937409202147,-0.087025242813415 51.4948093630698,-0.080948994382577 51.6393371189262))"),
                Attributes = new AttributesTable(new[]{ new KeyValuePair<string, object>("Name", "B") })
            },
            new Feature
            {
                Geometry = new WKTReader().Read("POLYGON((-0.28466193750684 51.4739437060513,-0.283274420066644 51.5108952170044,-0.206642056562167 51.5097485575542,-0.208091441424706 51.4727985538088,-0.28466193750684 51.4739437060513))"),
                Attributes = new AttributesTable(new[]{ new KeyValuePair<string, object>("Name", "C") })
            }
        };

        [Test]
        public void TestIntersection()
        {
            var intersection = _inputData.Intersection(_overlayData);

            verifyResult(intersection, new List<(string Id, string Name)>
            {
                { ("1", "A")},
                { ("2", "B")},
                { ("3", "C")},
                { ("4", "B")},
            });
        }

        [Test]
        public void TestDifference()
        {
            var difference = _inputData.Difference(_overlayData);

            verifyResult(difference, new List<(string Id, string Name)>
            {
                { ("1", null)},
                { ("2", null)},
                { ("3", null)},
                { ("4", null)},
            });

            var reverseDifference = _overlayData.Difference(_inputData);

            verifyResult(reverseDifference, new List<(string Id, string Name)>
            {
                { (null, "A")},
                { (null, "B")}
            });
        }

        [Test]
        public void TestUnion()
        {
            var union = _inputData.Union(_overlayData);

            verifyResult(union, new List<(string Id, string Name)>
            {
                { ("1", "A")},
                { ("2", "B")},
                { ("3", "C")},
                { ("4", "B")},
                { ("1", null)},
                { ("2", null)},
                { ("3", null)},
                { ("4", null)},
                { (null, "A")},
                { (null, "B")}
            });
        }

        [Test]
        public void TestSymDifference()
        {
            var difference = _inputData.SymDifference(_overlayData);

            verifyResult(difference, new List<(string Id, string Name)>
            {
                { ("1", null)},
                { ("2", null)},
                { ("3", null)},
                { ("4", null)},
                { (null, "A")},
                { (null, "B")}
            });
        }

        void verifyResult(IEnumerable<Feature> result, List<(string Id, string Name)> pairs)
        {
            var resultFeatures = result.ToList();
            var geometry = new GeometryCollection(resultFeatures.Select(g => g.Geometry).ToArray()).AsText();

            var resultPairs = resultFeatures.Select(r => (
                                                            Id: r.Attributes.Exists("ID") ? r.Attributes["ID"].ToString() : null,
                                                            Name: r.Attributes.Exists("Name") ? r.Attributes["Name"].ToString() : null
                                                         )
                                            )
                                            .ToList();

            CollectionAssert.AreEquivalent(pairs, resultPairs);
            
            
        }
    }
}
