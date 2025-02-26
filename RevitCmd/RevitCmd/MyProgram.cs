﻿using Autodesk.Revit.DB;

namespace RevitCmd
{
    public static class MyProgram
    {
        static string _docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static void DoStuff(Document document)
        {
            //var json = System.IO.File.ReadAllText(_docPath + "/model.json");

            //var model = JsonConvert.DeserializeObject<IModel>(json);
            var model = new Model(new List<IElement>
                {
                    //todo
                    new WallElement(new Vector{ X= 0, Y=0,Z=0 }, new Vector{X = 1, Y =0, Z= 0 }, new Dimension{ X= 10000, Y = 500, Z = 3000 }),
                    // new WallElement(new Vector{ X= 0, Y=0,Z=0 }, new Vector{X = -1, Y =0, Z= 0 }, new Dimension{ X= 500, Y = 5000, Z = 3000 }),
                    new RoofElement(new Vector{ X= 0, Y=0,Z=0 }, new Vector{X = 1, Y =0, Z= 0 }, new RoofShape{ X= 10000, Y = 5000, Z = 2000, AngleA = 30, AngleB = 20 })
                });

            document.Transaction(_ =>
            {
                model.Draw(document);
            });

            var json = model.SerializeJson();

            var model2 = json.DeserializeJson<ModelDtO>();

            document.Transaction(_ =>
            {
                var walls = document.QuOfType<Wall>();
                foreach (var wall in walls)
                {
                    var geo = wall.get_Geometry(
                        new Options()
                        {
                            View = MyExternalCommand.Application.ActiveUIDocument.ActiveGraphicalView,
                            ComputeReferences = true, // this is important since we need references for measuring
                            IncludeNonVisibleObjects = false
                        });
                    var solid = geo.OfType<Solid>().First();

                    var bottomFace = solid.Faces.OfType<Face>()
                                    .Where(o => o.ComputeNormal(new UV(.5, .5)).Z < 0).First(); // get bottom face
                    var ordered = bottomFace
                        .EdgeLoops.OfType<EdgeArray>()
                        .SelectMany(o => o.OfType<Edge>())
                        .OrderBy(o => ((Edge)o).ApproximateLength);
                    // first 2 smallest opposit
                    // last 2 longest opposit
                    var edge0 = ordered.ElementAt(0);
                    var edge1 = ordered.ElementAt(1);
                    var line = ordered.ElementAt(2).AsCurve() as Line;
                    double offset = 2000;

                    document.CreateDimension(edge0, edge1, line, offset);
                }

            });
            //todo: attach walls to roof!
            // Done! Great!



        }


    }
}
