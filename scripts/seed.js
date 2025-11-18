// Seed script for mongosh
// Usage:
//   mongosh "<MONGO_CONN>/<DB_NAME>" scripts/seed.js --eval "var RESET=false; var OWNERS=100; var PROPS=300; var IMAGES=600;"
// If variables are not provided via --eval, defaults are used.

(function () {
  const RESET = (typeof globalThis.RESET !== 'undefined') ? (globalThis.RESET === true || globalThis.RESET === 'true') : false;
  const OWNERS = Number(globalThis.OWNERS || 100);
  const PROPS = Number(globalThis.PROPS || 300);
  const IMAGES = Number(globalThis.IMAGES || 600);
  const TRACES_PER_PROPERTY = Number(globalThis.TRACES || 3); // promedio de trazas por propiedad

  const SINGLE_MODE = (typeof globalThis.SINGLE_MODE !== 'undefined') ? (globalThis.SINGLE_MODE === true || globalThis.SINGLE_MODE === 'true') : false;
  const SINGLE_NAME = globalThis.SINGLE_PROPERTY_NAME || "Propiedad Benchmark";
  const SINGLE_PRICE = Number(globalThis.SINGLE_PROPERTY_PRICE || 9876543);
  const SINGLE_IMAGES = Number(globalThis.SINGLE_PROPERTY_IMAGES || 50);

  // Helpers
  function randomChoice(arr) { return arr[Math.floor(Math.random() * arr.length)]; }
  function randomInt(min, max) { return Math.floor(Math.random() * (max - min + 1)) + min; }
  function pad(num, size) { let s = String(num); while (s.length < size) s = '0' + s; return s; }

  const firstNames = ["Juan","María","Carlos","Ana","Luis","Sofía","Jorge","Lucía","Miguel","Elena","Pedro","Laura","Diego","Camila","Andrés","Valentina"];
  const lastNames = ["Pérez","García","López","Martínez","Rodríguez","Hernández","González","Sánchez","Ramírez","Torres","Flores","Rivera","Gómez","Díaz","Vargas","Cruz"];
  const streets = ["Av. Principal","Calle Secundaria","Boulevard Central","Av. Costera","Calle de las Flores","Av. del Sol","Calle 5","Calle 20","Av. Reforma","Av. Libertad","Calle Palma","Av. Insurgentes"];
  const cities = ["Ciudad de México","Guadalajara","Monterrey","Cancún","Tijuana","Puebla","León","Querétaro","Mérida","Toluca"];
  const propertyImageCategories = ["house", "building", "apartment", "interior", "home", "architecture", "real-estate", "property"];
  const traceConcepts = [
    "Compra inicial",
    "Remodelación mayor",
    "Ajuste de avalúo",
    "Venta parcial",
    "Actualización de escrituras",
    "Renovación de hipoteca",
    "Ampliación del inmueble",
    "Cambio de propietario"
  ];

  const ownersCount = SINGLE_MODE ? 1 : OWNERS;
  const propsCount = SINGLE_MODE ? 1 : PROPS;
  const imagesCount = SINGLE_MODE ? SINGLE_IMAGES : IMAGES;

  print(`\n🚀 Iniciando seed en base de datos: ${db.getName()}`);
  print(`Parámetros => RESET:${RESET} OWNERS:${ownersCount} PROPS:${propsCount} IMAGES:${imagesCount} TRACES:${TRACES_PER_PROPERTY} SINGLE_MODE:${SINGLE_MODE}`);
  if (SINGLE_MODE) {
    print(`🔖 Propiedad especial => Nombre:"${SINGLE_NAME}" Precio:${SINGLE_PRICE} Imágenes:${SINGLE_IMAGES}`);
  }

  if (RESET) {
    print("🧹 Limpiando colecciones...");
    db.Owners.deleteMany({});
    db.Properties.deleteMany({});
    db.PropertyImages.deleteMany({});
    db.PropertyTraces.deleteMany({});
  }

  // 1) Owners
  const ownerDocs = [];
  for (let i = 1; i <= ownersCount; i++) {
    const fn = randomChoice(firstNames);
    const ln = randomChoice(lastNames);
    ownerDocs.push({
      IdOwner: `OWNER${pad(i, 4)}`,
      Name: `${fn} ${ln}`,
      Address: `${randomChoice(streets)} ${randomInt(1, 999)}, ${randomChoice(cities)}`,
      Photo: `https://loremflickr.com/200/200/person?random=${i}`,
      Birthday: new Date(1970 + randomInt(0, 35), randomInt(0, 11), randomInt(1, 28))
    });
  }
  print(`📥 Insertando ${ownerDocs.length} owners...`);
  const ownersResult = db.Owners.insertMany(ownerDocs);
  const ownerIds = Object.values(ownersResult.insertedIds);

  // 2) Properties
  const propertyDocs = [];
  if (SINGLE_MODE) {
    const ownerId = ownerIds[0];
    propertyDocs.push({
      IdProperty: "PROP-SPECIAL-001",
      Name: SINGLE_NAME,
      Address: "Av. Escalabilidad 5000, Ciudad Benchmark",
      Price: SINGLE_PRICE,
      CodeInternal: "INT-SPECIAL-001",
      Year: randomInt(2010, 2024),
      IdOwner: ownerId.str || ownerId.toString()
    });
  } else {
    for (let i = 1; i <= propsCount; i++) {
      const ownerId = ownerIds[randomInt(0, ownerIds.length - 1)];
      propertyDocs.push({
        IdProperty: `PROP${pad(i, 5)}`,
        Name: `${randomChoice(["Casa","Departamento","Apartamento","Loft","Condominio"])} ${randomChoice(["moderno","amplio","luminoso","acogedor"])}`,
        Address: `${randomChoice(streets)} ${randomInt(1, 999)}, ${randomChoice(cities)}`,
        Price: Number((randomInt(50, 500) * 1000).toFixed(2)),
        CodeInternal: `INT-${pad(i, 5)}`,
        Year: randomInt(1990, 2024),
        IdOwner: ownerId.str || ownerId.toString()
      });
    }
  }
  print(`📥 Insertando ${propertyDocs.length} properties...`);
  const propsResult = db.Properties.insertMany(propertyDocs);
  const propIds = Object.values(propsResult.insertedIds);

  // 3) PropertyImages
  const imageDocs = [];
  if (SINGLE_MODE) {
    const specialPropId = propIds[0].str || propIds[0].toString();
    for (let i = 1; i <= imagesCount; i++) {
      imageDocs.push({
        IdPropertyImage: `IMGSPEC${pad(i, 5)}`,
        IdProperty: specialPropId,
        file: `https://loremflickr.com/1200/800/${randomChoice(propertyImageCategories)}?random=${i}`,
        Enabled: true
      });
    }
  } else {
    for (let i = 1; i <= imagesCount; i++) {
      const propId = propIds[randomInt(0, propIds.length - 1)];
      imageDocs.push({
        IdPropertyImage: `IMG${pad(i, 6)}`,
        IdProperty: propId.str || propId.toString(),
        file: `https://loremflickr.com/800/600/${randomChoice(propertyImageCategories)}?random=${i}`,
        Enabled: Math.random() < 0.8
      });
    }
  }
  print(`📥 Insertando ${imageDocs.length} property images...`);
  db.PropertyImages.insertMany(imageDocs);

  // 4) PropertyTraces
  const traceDocs = [];
  let traceCounter = 1;
  propertyDocs.forEach((property, index) => {
    const propMongoId = propIds[index].str || propIds[index].toString();
    const traceCount = Math.max(0, randomInt(TRACES_PER_PROPERTY - 1, TRACES_PER_PROPERTY + 1));

    for (let t = 0; t < traceCount; t++) {
      const saleYear = Math.min(
        new Date().getFullYear(),
        Math.max(property.Year, property.Year + randomInt(0, 5))
      );
      const saleDate = new Date(saleYear, randomInt(0, 11), randomInt(1, 28));
      const saleValue = Number((property.Price * (0.85 + Math.random() * 0.3)).toFixed(2));
      const taxRate = randomChoice([0.05, 0.07, 0.08, 0.1]);
      const taxValue = Number((saleValue * taxRate).toFixed(2));

      traceDocs.push({
        IdPropertyTrace: `TRACE${pad(traceCounter++, 6)}`,
        DateSale: saleDate,
        Name: randomChoice(traceConcepts),
        Value: saleValue,
        Tax: taxValue,
        IdProperty: propMongoId
      });
    }
  });

  if (traceDocs.length > 0) {
    print(`📥 Insertando ${traceDocs.length} property traces...`);
    db.PropertyTraces.insertMany(traceDocs);
  } else {
    print("ℹ️ Sin property traces generadas (configuración actual).");
  }

  print("✅ Seed completado exitosamente\n");
})();
