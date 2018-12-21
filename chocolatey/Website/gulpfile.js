"use strict";

const gulp = require("gulp"),
    del = require("del"),
    concat = require('gulp-concat'),
    cleanCSS = require('gulp-clean-css'),
    purgecss = require("gulp-purgecss"),
    sass = require("gulp-sass"),
    dist = "Content/dist";
    sass.compiler = require('node-sass');

function clean() {
    return del(dist);
}

function compileSASS() {
    return gulp.src("Content/scss/*.scss")
        .pipe(sass().on('error', sass.logError))
        .pipe(gulp.dest(dist));
}

function purge() {
    return gulp.src("Content/dist/chocolatey.css")
        .pipe(purgecss({
            content: ["Views/**/*.cshtml", "App_Code/ViewHelpers.cshtml", "Errors/*.*", "Scripts/custom.js", "Scripts/packages/package-details.js"]
        }))
        .pipe(gulp.dest("Content/dist/tmp"));
}

function optimize() {
    return gulp.src(["Content/dist/tmp/chocolatey.css", "Content/dist/purge.css"])
        .pipe(concat("chocolatey.slim.css"))
        .pipe(cleanCSS({
            level: 1,
            compatibility: 'ie8'
        }))
        .pipe(gulp.dest(dist))
        .on('end', function () {
            // To keep chocolatey.css full, uncomment last item in del array
            del(["Content/dist/purge.css", "Content/dist/tmp", "Content/dist/chocolatey.css"]);
        });
}

// Task
gulp.task("default", gulp.series(clean, compileSASS, purge, optimize));