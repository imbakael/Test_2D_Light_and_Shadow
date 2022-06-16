using System.Collections.Generic;
using UnityEngine;

namespace FunkyCode.SpriteExtension {
    public static class PhysicsShapeManager {
        static public Dictionary<Sprite, PhysicsShape> dictionary = new Dictionary<Sprite, PhysicsShape>();

        static public void Clear() {
            dictionary = new Dictionary<Sprite, PhysicsShape>();
        }

        static public PhysicsShape RequestCustomShape(Sprite originalSprite) {
            if (originalSprite == null) {
                return null;
            }

            bool exist = dictionary.TryGetValue(originalSprite, out PhysicsShape shape);

            if (exist) {
                if (shape == null || shape.GetSprite().texture == null) {
                    shape = RequestCustomShapeAccess(originalSprite);
                }
                return shape;
            } else {
                return RequestCustomShapeAccess(originalSprite);
            }
        }

        static public PhysicsShape RequestCustomShapeAccess(Sprite originalSprite) {

            bool exist = dictionary.TryGetValue(originalSprite, out PhysicsShape shape);

            if (exist) {
                if (shape == null || shape.GetSprite().texture == null) {
                    dictionary.Remove(originalSprite);

                    shape = AddShape(originalSprite);

                    dictionary.Add(originalSprite, shape);
                }

                return shape;
            } else {
                shape = AddShape(originalSprite);

                dictionary.Add(originalSprite, shape);

                return shape;
            }
        }

        static private PhysicsShape AddShape(Sprite sprite) {
            if (sprite == null || sprite.texture == null) {
                return null;
            }

            var shape = new PhysicsShape(sprite);

            return shape;
        }
    }
}